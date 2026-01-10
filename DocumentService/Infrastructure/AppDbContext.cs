using DocumentService.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options):base(options){}
    
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ers");

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.ProjectId);
            entity.Property(d => d.Status).IsRequired();
        });
        
        base.OnModelCreating(modelBuilder);
    }
}