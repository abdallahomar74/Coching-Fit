﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<PersonalizedPlan> PersonalizedPlans { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<SubscriptionPlan>()
             .Property(sp => sp.Price)
             .HasColumnType("decimal(18,2)");

            builder.Entity<CoachSubscription>()
        .HasOne(cs => cs.SubscriptionPlan)
        .WithMany() 
        .HasForeignKey(cs => cs.SubscriptionPlanId)
        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ChatMessage>()
        .HasOne(cm => cm.Subscription)
        .WithMany() 
        .HasForeignKey(cm => cm.CoachSubscriptionId)
        .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
