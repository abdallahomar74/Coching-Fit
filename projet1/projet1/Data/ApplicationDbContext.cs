using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using projet1.Data.Models;
using projet1.Models;
using System.Reflection.Emit;

namespace projet1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<CoachSubscription> coachsubscriptions { get; set; }
        public DbSet<SubscriptionPlan> subscriptionplans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<SubscriptionPlan>()
             .Property(sp => sp.Price)
             .HasColumnType("decimal(18,2)");

            builder.Entity<CoachSubscription>()
        .HasOne(cs => cs.SubscriptionPlan)
        .WithMany() // إذا كان لديك مجموعة من الاشتراكات في SubscriptionPlan يمكنك استخدام .WithMany(sp => sp.CoachSubscriptions)
        .HasForeignKey(cs => cs.SubscriptionPlanId)
        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
