using Microsoft.EntityFrameworkCore;

namespace Tags.Api.Data;

public class TagDbContext(DbContextOptions<TagDbContext> options) : DbContext(options)
{
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Color).IsRequired().HasMaxLength(50);
            entity.Property(t => t.CreatedAtUtc).IsRequired();
            entity.Property(t => t.NoteId).IsRequired();

            entity.HasIndex(t => t.Id);
        });
    }
}
