using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutritionAI.Models;

namespace NutritionAI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Meal> Meals { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Meal -> Foods (Cascade Delete)
            builder.Entity<Food>()
                .HasOne(f => f.Meal)
                .WithMany(m => m.Foods)
                .HasForeignKey(f => f.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> UserProfile (One-to-One)
            builder.Entity<UserProfile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            builder.Entity<UserProfile>()
                .HasIndex(p => p.UserEmail)
                .IsUnique();

            // User -> Meals (One-to-Many)
            builder.Entity<Meal>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
