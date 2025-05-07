using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public string ImagePath { get; set; } // Path to the image

        public bool IsMainImage { get; set; } = false;

        [ForeignKey("ProductID")]

        public int ProductID { get; set; }
    }

}
