using Microsoft.EntityFrameworkCore;

namespace BachHoaXanh.Data
{
    using BachHoaXanh.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> UserList { get; set; }
        public DbSet<FaceData> FaceData { get; set; }
        public DbSet<FaceAuthHistory> FaceAuthHistory { get; set; }

        public DbSet<Category> CategoryList { get; set; }
        public DbSet<SubCategory> SubCategoryList { get; set; }

        public DbSet<Product> ProductList { get; set; }
        public DbSet<Review> ReviewList { get; set; }
        public DbSet<FavoriteProduct> FavoriteProductList { get; set; }
        public DbSet<StockProduct> StockProductList { get; set; }
        public DbSet<ProductImage> ProductImageList { get; set; }

        public DbSet<Order> OrderList { get; set; }
        public DbSet<OrderDetail> OrderDetailList { get; set; }
        public DbSet<OrderStockDetail> OrderStockDetailList { get; set; }

        public DbSet<PaymentMethod> PaymentMethodList { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FaceData>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID);

            // Configure decimal precision
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(u => u.Points)
                .HasPrecision(18, 2);

            // Configure CanCancel default value
            modelBuilder.Entity<Order>()
                .Property(o => o.CanCancel)
                .HasDefaultValue(true);
        }

        public void UpdateOrderCanCancelDefault()
        {
            Database.ExecuteSqlRaw("UPDATE Orders SET CanCancel = 1 WHERE CanCancel IS NULL");
        }
    }
}