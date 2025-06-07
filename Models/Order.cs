using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [ForeignKey("PaymentMethod")]
        public int PaymentMethodID { get; set; }

        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }

        [StringLength(50)]
        public string? OrderStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
        public bool CanCancel { get; set; } = true; // mặc định có thể hủy
        public User? User { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}