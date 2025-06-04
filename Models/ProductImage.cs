using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public bool IsMainImage { get; set; } = false;

        [ForeignKey("ProductID")]
        public int ProductID { get; set; }
    }
}