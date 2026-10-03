using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Infrastructure.Data;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
        public DbSet<User> Users => Set<User>();
        public DbSet<Link> Links => Set<Link>();
        public DbSet<Click> Clicks => Set<Click>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e=>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Link>(e=>
            {
                e.HasKey(l => l.Id);
                e.HasIndex(l => l.ShortCode).IsUnique();
                e.HasIndex(l => l.UserId);
                e.Property(l => l.OriginalUrl).IsRequired().HasMaxLength(2048);
                e.Property(l => l.ShortCode).HasMaxLength(16).IsRequired();

                e.HasOne(l => l.User)
                    .WithMany(u => u.Links)
                    .HasForeignKey(l => l.UserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Click>(e=>
            {
                e.HasKey(c => c.Id);
                e.HasIndex(c => c.LinkId);

                e.HasOne(c => c.link)
                    .WithMany(l => l.Clicks)
                    .HasForeignKey(c => c.LinkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
