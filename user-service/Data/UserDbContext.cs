using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        user.ToTable("dotnet_users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).UseIdentityAlwaysColumn();
        user.Property(x => x.Username).HasMaxLength(20).IsRequired();
        user.Property(x => x.Email).HasMaxLength(255).IsRequired();
        user.Property(x => x.FullName).HasMaxLength(100);
        user.Property(x => x.Phone).HasMaxLength(15);
        user.Property(x => x.IsActive).HasDefaultValue(true);
        user.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        user.HasIndex(x => x.Username).IsUnique();
        user.HasIndex(x => x.Email).IsUnique();
    }
}
