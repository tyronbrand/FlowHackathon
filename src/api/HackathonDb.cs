using Hackathon.Api.Users;
using Microsoft.EntityFrameworkCore;

public class HackathonDb : DbContext
{
    public HackathonDb(DbContextOptions options) : base(options) { }
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasIndex(u => u.UnityId)
            .IsUnique();
    }
}