using System.Net;
using System.Text.RegularExpressions;
using Domain.ValueObject;
using Infrastructure.Actors.PageIndex;

namespace Wiki.Services;

public class WikiLinkParser
{
    // Matches:
    // [[3e4ba077-7e54-493e-a77f-b9c77641ad9d]]
    // [[3e4ba077-7e54-493e-a77f-b9c77641ad9d:link title]]
    //
    // Notes:
    // - Title is optional
    // - Title can contain colons; only the FIRST colon splits guid vs title
    private static readonly Regex WikiLinkRegex = new(
        pattern: @"\[\[(?<id>[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})(?::(?<title>[^\]]+))?\]\]",
        options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Replaces wiki links with HTML anchor tags.
    /// </summary>
    /// <param name="input">Text containing wiki links.</param>
    /// <param name="pageIndexGrain"></param>
    /// <param name="hrefBuilder">Optional builder for hrefs. Default: /view/{guid}</param>
    public string ToHtmlLinks(string input, IPageIndexGrain pageIndexGrain, Func<Guid, string>? hrefBuilder = null)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        hrefBuilder ??= static g => $"/view/{g:D}";

        return WikiLinkRegex.Replace(input, match =>
        {
            var idText = match.Groups["id"].Value;

            // Should always succeed due to regex, but keep it safe.
            if (!Guid.TryParse(idText, out var id))
                return match.Value;

            var titleGroup = match.Groups["title"];
            var pageIndexEntry = pageIndexGrain.GetById(id.ToString()).GetAwaiter().GetResult();
            var slug = new Slug(pageIndexEntry?.Title ?? string.Empty).SlugValue;
            var rawTitle = titleGroup.Success ? titleGroup.Value.Trim() : pageIndexEntry?.Title;

            // HTML-encode to avoid injection issues.
            var title = WebUtility.HtmlEncode(rawTitle);

            // Build href and attribute-encode quotes minimally.
            var href = hrefBuilder(id) ?? string.Empty;
            href = WebUtility.HtmlEncode(href);

            return $@"<a href=""{href}/{slug}"">{title}</a>";
        });
    }
}