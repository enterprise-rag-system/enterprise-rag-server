using Microsoft.EntityFrameworkCore;
using ProjectService.Models.Entities;

namespace ProjectService.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options):base(options){}
    
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ers");

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.OwnerUserId)
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            
        });

        // ProjectMember
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("project_members");

            entity.HasKey(pm => pm.Id);

            entity.Property(pm => pm.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(pm => pm.CreatedAt)
                .IsRequired();

            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}