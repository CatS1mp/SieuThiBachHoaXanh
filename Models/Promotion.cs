using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        public int PromotionID { get; set; }

        [Required]
        [MaxLength(250)]
        public string PromotionName { get; set; } = string.Empty;

        public ICollection<PromotionDetail>? PromotionDetails { get; set; }
    }
}
