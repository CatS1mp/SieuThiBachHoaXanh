using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(250)]
        public string? ProductName { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public ProductStatus Status { get; set; } // Sử dụng enum

        [NotMapped]
        public bool isFav { get; set; } = false;

        [ForeignKey("SubCategoryID")]
        public SubCategory? SubCategory { get; set; }
        public int? SubCategoryID { get; set; }
        public List<ProductImage>? Images { get; set; }

        public List<StockProduct> Stocks { get; set; } = new List<StockProduct>();

        [NotMapped]
        public int StockQuantity => Stocks?.Where(s => s.ExpirationDate >= DateTime.Now)
                                 .Sum(s => s.Quantity) ?? 0;


    }

    [Table("FavoriteProducts")]
    public class FavoriteProduct
    {
        [Key]
        public int FavoriteID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        [ForeignKey("UserID")]
        public User? Users { get; set; }
        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }
    public enum ProductStatus : byte
    {
        KinhDoanh = 0,
        TamHetHang = 1,
        NgungKinhDoanh = 2
    }

}
