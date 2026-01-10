
using ChatService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options):base(options){}
    
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ers");
        ConfigureChatMessage(modelBuilder);
    }

    private static void ConfigureChatMessage(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ChatMessage>();

        entity.ToTable("chat_messages");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedNever();

        entity.Property(x => x.ProjectId)
            .IsRequired();

        entity.Property(x => x.Sender)
            .IsRequired()
            .HasConversion<int>();

        entity.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(8000);

        entity.Property(x => x.CorrelationId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.Property(x => x.IsDeleted)
            .IsRequired();

        entity.Property(x => x.TokenUsage);

        entity.Property(x => x.SourceChunkIdsJson)
            .HasColumnType("jsonb");
    }
}
