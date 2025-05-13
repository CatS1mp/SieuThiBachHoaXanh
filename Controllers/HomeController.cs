using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

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
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }

        int userId = int.Parse(User.FindFirstValue("UserID"));

        var product = _context.ProductList
            .Include(p => p.SubCategory)
            .ThenInclude(sc => sc.Category)
            .Include(p => p.Images)
            .FirstOrDefault(p => p.ProductID == id);
        bool isFavorite = _context.FavoriteProductList
        .Any(p => p.ProductID == id && p.UserID == userId);
        if (product == null)
        {
            return NotFound();
        }
        var productViewModel = new ProductDetailViewModel
        {
            Product = product,
            isFav = isFavorite
        };
        return View(productViewModel);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToFavorites(int productId)
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }
        int userId = int.Parse(User.FindFirstValue("UserID"));
        var exists = await _context.FavoriteProductList
            .AnyAsync(f => f.UserID == userId && f.ProductID == productId);

        if (!exists)
        {
            _context.FavoriteProductList.Add(new FavoriteProduct
            {
                UserID = userId,
                ProductID = productId
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Detail", new { id = productId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RemoveFromFavorites(int productId)
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }
        int userId = int.Parse(User.FindFirstValue("UserID"));
        var favorite = await _context.FavoriteProductList
            .FirstOrDefaultAsync(f => f.UserID == userId && f.ProductID == productId);

        if (favorite != null)
        {
            _context.FavoriteProductList.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Detail", new { id = productId });
        }
}


