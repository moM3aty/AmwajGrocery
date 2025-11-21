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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, NameAr = "ألبان وأجبان", NameEn = "Dairy & Cheese", ImageUrl = "images/dairy.webp" },
                new Category { Id = 2, NameAr = "عصائر ومشروبات", NameEn = "Juices & Drinks", ImageUrl = "images/juice.webp" }
            );
        }
    }
}