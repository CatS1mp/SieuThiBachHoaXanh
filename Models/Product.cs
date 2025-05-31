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
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public bool isFav { get; set; } = false;

        [NotMapped]
        [Range(0, double.MaxValue)]
        public decimal PromotionPrice { get; set; } = -1;



        [ForeignKey("SubCategoryID")]
        public SubCategory SubCategory { get; set; }
        public int? SubCategoryID { get; set; }
        public List<ProductImage> Images { get; set; }
        

    }

    [Table("FavoriteProducts")]
    public class FavoriteProduct
    {
        [Key]
        public int FavoriteID { get; set; }
        public int UserID { get; set; } // Tham chiếu đến ASP.NET Identity User
        public int ProductID { get; set; }
        [ForeignKey("UserID")]
        public User Users { get; set; }  // Navigation property
        [ForeignKey("ProductID")]
        public Product Product { get; set; }       // Navigation property
    }


}
