using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }
    public IActionResult Index(int page = 1, string search = "", int? danhmuc = null)
    {
        int pageSize = 8;
        var categories = _context.CategoryList.Include(c => c.SubCategories).ToList();
        var productsQuery = _context.ProductList.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            productsQuery = productsQuery.Where(p => p.ProductName.Contains(search) || p.Description.Contains(search));
        }

        if (danhmuc.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.SubCategory.SubCategoryID == danhmuc.Value);
        }

        if (page < 1)
        {
            page = 1;
        }

        ViewData["Search"] = search;
        ViewData["SubCategoryId"] = danhmuc;

        int totalProducts = productsQuery.Count();
        int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
        }

        var products = productsQuery
            .Include(p => p.SubCategory)
            .ThenInclude(sc => sc.Category)
            .Include(p => p.Images)
            .Skip((page - 1) * pageSize) 
            .Take(pageSize)
            .ToList();

        var productViewModel = new ProductViewModel
        {
            Products = products,
            Categories = categories,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalProducts = totalProducts,
            SearchQuery = search,
            SubCategoryId = danhmuc
        };

        return View(productViewModel);
    }

    [Route("san-pham/{id}")]
    public IActionResult Detail(int id)
    {
        var product = _context.ProductList.Include(p => p.SubCategory)
                                           .ThenInclude(sc => sc.Category)
                                           .Include(p => p.Images)
                                           .FirstOrDefault(p => p.ProductID == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}
