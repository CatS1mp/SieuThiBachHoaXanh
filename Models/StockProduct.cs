using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("ProductStocks")]
    public class StockProduct
    {
        [Key]
        public int StockID { get; set; }

        [Required]
        [ForeignKey("Products")]
        public int ProductID { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; }
    }
}