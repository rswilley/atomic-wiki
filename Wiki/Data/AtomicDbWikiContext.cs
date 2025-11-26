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
    public DbSet<PageCategory> Categories => Set<PageCategory>();

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
        
        // Category
        modelBuilder.Entity<PageCategory>()
            .HasIndex(c => c.ExternalId)
            .IsUnique();

        // One-to-many Category -> Pages
        modelBuilder.Entity<Page>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Pages)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull); // When category deleted, keep pages but null out CategoryId

        // Many-to-many Page <-> Tag
        modelBuilder.Entity<Page>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Pages)
            .UsingEntity(j => j.ToTable("PageTags"));
    }
}