using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options) : base(options){}
    
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ers");
        
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
    }

}