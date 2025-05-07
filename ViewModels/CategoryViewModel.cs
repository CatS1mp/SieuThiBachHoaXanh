using BachHoaXanh.Models;
using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int? TotalProducts { get; set; }
        public int? TotalSubCat { get; set; }
        public List<SubCategoryViewModel> SubCategories { get; set; }
    }

    public class SubCategoryViewModel
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public int? TotalProducts { get; set; }
    }
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string CategoryName { get; set; }

    }
}
