using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.Models
{
    [Table("PromotionDetails")]
    public class PromotionDetail
    {
        [Key]
        public int PromotionDetailID { get; set; }

        [Required]
        public int PromotionID { get; set; }

        [Required]
        public int ProductID { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NewPrice { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PromotionID))]
        public Promotion? Promotion { get; set; }

        [ForeignKey(nameof(ProductID))]
        public Product? Product { get; set; }
    }
}