using Microsoft.EntityFrameworkCore;
using Wiki.Models;

namespace Wiki.Data;

public class AtomicWikiDbContext : DbContext
{
    public AtomicWikiDbContext(DbContextOptions<AtomicWikiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageTag> Tags => Set<PageTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraint on ExternalId
        modelBuilder.Entity<Page>()
            .HasIndex(p => p.ExternalId)
            .IsUnique();

        modelBuilder.Entity<PageTag>()
            .HasIndex(t => t.ExternalId)
            .IsUnique();

        // Many-to-many Page <-> Tag
        modelBuilder.Entity<Page>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Pages)
            .UsingEntity(j => j.ToTable("PageTags"));
    }
}