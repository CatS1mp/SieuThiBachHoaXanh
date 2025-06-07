using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

    }
    [Table("OrderStockDetails")]

    public class OrderStockDetail
    {
        [Key]
        public int OrderStockDetailID { get; set; }

        [ForeignKey("OrderDetail")]
        public int OrderDetailID { get; set; }

        [ForeignKey("StockProduct")]
        public int StockID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [JsonIgnore]
        public OrderDetail OrderDetail { get; set; }
        public StockProduct StockProduct { get; set; }
    }
}