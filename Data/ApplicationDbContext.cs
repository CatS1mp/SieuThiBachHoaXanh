using Microsoft.EntityFrameworkCore;

namespace BachHoaXanh.Data
{
    using BachHoaXanh.Models;
    using Microsoft.EntityFrameworkCore;

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
        public DbSet<StockProduct> StockProductList { get; set; }

        public DbSet<ProductImage> ProductImageList { get; set; }
        public DbSet<Order> OrderList { get; set; }
        public DbSet<OrderDetail> OrderDetailList { get; set; }
        public DbSet<PaymentMethod> PaymentMethodList { get; set; }
        public DbSet<Address> Addresses { get; set; }

    }

}
