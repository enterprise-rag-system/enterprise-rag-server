using Microsoft.EntityFrameworkCore;
using RagWorker.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace RagWorker.Infrastructure.Persistence;

public class RagDbContext : DbContext
{
    private readonly int _vectorDimensions;

    public RagDbContext(
        DbContextOptions<RagDbContext> options,
        IConfiguration configuration)
        : base(options)
    {
        _vectorDimensions =
            configuration.GetValue<int>("VectorStore:Dimensions");
    }

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<AiExecutionLog> AiExecutionLogs => Set<AiExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("document_chunks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Embedding)
                .HasColumnType("vector(3072)");

            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.DocumentId);
        });
    }
}