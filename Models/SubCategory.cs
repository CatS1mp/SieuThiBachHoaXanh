using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("SubCategories")]
    public class SubCategory
    {
        [Key]
        public int SubCategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string? SubCategoryName { get; set; }
        public int? CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}