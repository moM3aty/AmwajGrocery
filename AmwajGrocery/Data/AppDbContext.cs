using AmwajGrocery.Models;
using Microsoft.EntityFrameworkCore;

namespace AmwajGrocery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SiteSetting>().HasData(
                new SiteSetting
                {
                    Id = 1,
                    BannerTextAr = "✨ عروض موسمية وتخفيضات خاصة بانتظارك! تسوق الآن ووفر المزيد. ✨",
                    BannerTextEn = "✨ Seasonal offers and special discounts await! Shop now and save more. ✨"
                }
            );
        }
    }
}