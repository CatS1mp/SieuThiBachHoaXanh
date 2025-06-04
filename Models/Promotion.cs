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


        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(250)]
        public string ImagePath { get; set; } = string.Empty;

        public bool ShowOnTop { get; set; } = false;

        public ICollection<PromotionDetail> PromotionDetails { get; set; } = new List<PromotionDetail>();

    }
}
