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
<<<<<<< Updated upstream
        public string ProductName { get; set; }

        public string Description { get; set; }
=======
        public string? ProductName { get; set; } = "";

        public string? Description { get; set; } = "";
>>>>>>> Stashed changes

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public bool isFav { get; set; } = false;

        [ForeignKey("SubCategoryID")]
        public SubCategory SubCategory { get; set; }
        public int? SubCategoryID { get; set; }
        public List<ProductImage> Images { get; set; }
        

<<<<<<< Updated upstream
=======
        public List<StockProduct> Stocks { get; set; } = new List<StockProduct>();

        [NotMapped]
        public int StockQuantity => Stocks?.Where(s => s.ExpirationDate >= DateTime.Now)
                                 .Sum(s => s.Quantity) ?? 0;


        [NotMapped]
        [Range(0, double.MaxValue)]
        public decimal PromotionPrice { get; set; } = -1;


        // Removed: [ForeignKey("ProductID")]
        // Removed: public FavoriteProduct? product { get; set; }
>>>>>>> Stashed changes
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
