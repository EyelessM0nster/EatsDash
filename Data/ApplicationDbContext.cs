using EatsDash.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EatsDash.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<ReviewReaction> ReviewReactions => Set<ReviewReaction>();
    public DbSet<ReviewReport> ReviewReports => Set<ReviewReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Courier>(entity =>
        {
            entity.Property(c => c.Nickname).HasMaxLength(80);
            entity.HasIndex(c => c.Nickname);
        });

        builder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => r.CreatedAt);
            entity.HasIndex(r => r.Status);
            entity.Property(r => r.AuthorName).HasMaxLength(100);
            entity.Property(r => r.CourierNickname).HasMaxLength(80);
            entity.Property(r => r.Text).HasMaxLength(2000);
            entity.HasOne(r => r.Author)
                .WithMany()
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Courier)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ReviewReaction>(entity =>
        {
            entity.HasIndex(r => new { r.ReviewId, r.UserId }).IsUnique();
            entity.HasOne(r => r.Review)
                .WithMany(review => review.Reactions)
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewReport>(entity =>
        {
            entity.Property(r => r.Reason).HasMaxLength(200);
            entity.Property(r => r.Comment).HasMaxLength(500);
            entity.HasIndex(r => new { r.ReviewId, r.ReporterId });
            entity.HasOne(r => r.Review)
                .WithMany()
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
