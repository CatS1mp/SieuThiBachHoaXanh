using Microsoft.AspNetCore.Mvc;

using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using System.Linq;
using BachHoaXanh.Data;

namespace BachHoaXanh.Controllers
{
    public class SharedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SharedController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Categories()
        {
            var categories = _context.CategoryList
                .Select(category => new CategoryViewModel
                {
                    CategoryID = category.CategoryID,
                    CategoryName = category.CategoryName,
                    SubCategories = category.SubCategories
                        .Select(sub => new SubCategoryViewModel
                        {
                            SubCategoryID = sub.SubCategoryID,
                            SubCategoryName = sub.SubCategoryName
                        }).ToList()
                }).ToList();

            return PartialView("_CategoriesPartial", categories);
        }
    }
}
