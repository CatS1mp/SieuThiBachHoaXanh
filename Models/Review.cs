using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("Reviews")]

    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required]
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public User? User { get; set; }
        public Product? Product { get; set; }


    }
}
