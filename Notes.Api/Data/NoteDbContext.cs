using Microsoft.EntityFrameworkCore;

namespace Notes.Api.Data;

public class NoteDbContext(DbContextOptions<NoteDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Note>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id).ValueGeneratedOnAdd();
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Content).IsRequired();
            entity.Property(n => n.CreatedAtUtc).IsRequired();
            entity.Property(n => n.LastUpdatedAtUtc).IsRequired();

            entity.HasIndex(n => n.Id);
        });
    }
}
