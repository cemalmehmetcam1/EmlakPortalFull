using EmlakPortal.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmlakPortal.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<EstateImage> EstateImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Estate> Estates { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Estate>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Estates)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

  
            builder.Entity<Estate>()
                .Property(e => e.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EstateImage>()
    .HasOne(ei => ei.Estate)
    .WithMany(e => e.Images)
    .HasForeignKey(ei => ei.EstateId)
    .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Favorite>()
    .HasOne(f => f.User)
    .WithMany(u => u.Favorites)
    .HasForeignKey(f => f.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasOne(f => f.Estate)
                .WithMany()  
                .HasForeignKey(f => f.EstateId)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}