using Domain;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;
using OpenMode = Lucene.Net.Index.OpenMode;

namespace Infrastructure.Repositories;

public interface ISearchRepository
{
    void Create(PageSearchItem item);
    void Update(PageSearchItem item);
    void DeleteById(string permanentId);
    SearchResult Search(string searchQuery);
}

public class LuceneRepository(IConfigurationService configurationService) : ISearchRepository
{
    private readonly string _indexPath = configurationService.GetSearchIndexDirectory();
    private readonly LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;
    
    public void Create(PageSearchItem item)
    {
        using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
        using IndexWriter writer = new IndexWriter(indexDir, GetIndexWriterConfig().config);

        writer.AddDocument(item.ToDocument());

        //Flush and commit the index data to the directory
        writer.Commit();
    }

    public void Update(PageSearchItem item)
    {
        using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
        using IndexWriter writer = new IndexWriter(indexDir, GetIndexWriterConfig().config);

        writer.UpdateDocument(new Term(nameof(PageSearchItem.PermanentId), item.PermanentId), item.ToDocument());
        writer.Commit();
    }

    public void DeleteById(string permanentId)
    {
        using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
        using IndexWriter writer = new IndexWriter(indexDir, GetIndexWriterConfig().config);

        writer.DeleteDocuments(new Term(nameof(PageSearchItem.PermanentId), permanentId));

        //Flush and commit the index data to the directory
        writer.Commit();
    }

    public SearchResult Search(string searchQuery)
    {
        using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
        using var reader = DirectoryReader.Open(indexDir);
        IndexSearcher searcher = new IndexSearcher(reader);

        var analyzer = GetStandardAnalyzer();
        var parser = new MultiFieldQueryParser(
            _luceneVersion,
            [
                nameof(PageSearchItem.Title), 
                nameof(PageSearchItem.Body),
                nameof(PageSearchItem.Headings),
                nameof(PageSearchItem.Tags),
            ],
            analyzer, new Dictionary<string, float>
            {
                { nameof(PageSearchItem.Title), 4.0f },
                { nameof(PageSearchItem.Headings), 2.5f },
                { nameof(PageSearchItem.Tags), 2.0f },
                { nameof(PageSearchItem.Body), 1.0f }
            }
        );

        Query query;
        try
        {
            query = parser.Parse(searchQuery);
        }
        catch (ParseException)
        {
            // Fallback for invalid query syntax
            query = parser.Parse(QueryParserBase.Escape(searchQuery));
        }

        TopDocs topDocs = searcher.Search(query, int.MaxValue);
        var snippets = GetSnippets(query, topDocs, searcher, analyzer);

        return new SearchResult
        {
            TotalCount = topDocs.TotalHits,
            Snippets = snippets
        };
    }
    
    private static List<SearchSnippet> GetSnippets(Query query, TopDocs topDocs, IndexSearcher searcher, Lucene.Net.Analysis.Analyzer analyzer)
    {
        var snippets = new List<SearchSnippet>();
        var scorer = new QueryScorer(query);
        var highlighter = new Highlighter(new SimpleHTMLFormatter("<b>", "</b>"), scorer)
        {
            // Set fragment size (characters)
            TextFragmenter = new SimpleFragmenter(150)
        };

        foreach (var scoreDoc in topDocs.ScoreDocs)
        {
            var doc = searcher.Doc(scoreDoc.Doc);
            var content = doc.Get(nameof(PageSearchItem.Body));

            // Extract a fragment with highlighting
            using var tokenStream = analyzer.GetTokenStream(nameof(PageSearchItem.Body), content);
            var snippet = highlighter.GetBestFragments(tokenStream, content, 1, "...");

            // If no highlight was found, use the beginning of the content
            if (string.IsNullOrEmpty(snippet) && !string.IsNullOrEmpty(content))
            {
                snippet = content.Length > 150 ? string.Concat(content.AsSpan(0, 150), "...") : content;
            }

            var pageSearchItem = PageSearchItem.ToPageSearchItem(doc);

            snippets.Add(new SearchSnippet
            {
                PermanentId = pageSearchItem.PermanentId,
                Title = pageSearchItem.Title,
                Snippet = snippet
            });
        }

        return snippets;
    }

    private (IndexWriterConfig config, Lucene.Net.Analysis.Analyzer analyzer) GetIndexWriterConfig()
    {
        var standardAnalyzer = GetStandardAnalyzer();
        return (new IndexWriterConfig(_luceneVersion, standardAnalyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        }, standardAnalyzer);
    }

    private Lucene.Net.Analysis.Analyzer GetStandardAnalyzer()
    {
        return new StandardAnalyzer(_luceneVersion);
    }
}

public class SearchResult
{
    public int TotalCount { get; init; }
    public IEnumerable<SearchSnippet>? Snippets { get; init; }
}

public class SearchSnippet
{
    public required string PermanentId { get; init; }
    public required string Title { get; init; }
    public required string Snippet { get; init; }
}

public class PageSearchItem
{
    public required string PermanentId { get; set; }
    public string Title { get; set; } = "";
    public List<string> Headings { get; set; } = [];
    public string Body { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    
    public Document ToDocument()
    {
        var doc = new Document
        {
            new StringField(nameof(PermanentId), PermanentId, Field.Store.YES),
            new TextField(nameof(Title), Title, Field.Store.YES),
            new TextField(nameof(Body), Body, Field.Store.YES)
        };

        foreach (var heading in Headings)
        {
            doc.Fields.Add(new StringField(nameof(Headings), heading, Field.Store.YES));
        }
        
        foreach (var tag in Tags)
        {
            doc.Fields.Add(new StringField(nameof(Tags), tag, Field.Store.YES));
        }

        return doc;
    }
    
    public static PageSearchItem ToPageSearchItem(Document document)
    {
        var headings = new List<string>();
        foreach (var field in document.Fields.Where(f => f.Name == nameof(Headings)))
        {
            headings.Add(field.GetStringValue());
        }
        
        var tags = new List<string>();
        foreach (var field in document.Fields.Where(f => f.Name == nameof(Tags)))
        {
            tags.Add(field.GetStringValue());
        }

        return new PageSearchItem
        {
            PermanentId = document.Get(nameof(PermanentId)),
            Title = document.Get(nameof(Title)),
            Headings = headings,
            Tags = tags,
            Body = document.Get(nameof(Body))
        };
    }
}