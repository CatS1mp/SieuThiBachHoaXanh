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
        public DbSet<FavoriteProduct> FavoriteProductList { get; set; }
        public DbSet<PromotionDetail> PromotionDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<StockProduct> StockProductList { get; set; }
        public DbSet<ProductImage> ProductImageList { get; set; }
        public DbSet<Order> OrderList { get; set; }
        public DbSet<OrderDetail> OrderDetailList { get; set; }
        public DbSet<PaymentMethod> PaymentMethodList { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình quan hệ nếu cần
            modelBuilder.Entity<FaceData>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID);
        }
    }
}