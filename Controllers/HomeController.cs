using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


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

        int userId = int.TryParse(User.FindFirstValue("UserID"), out int parsedId) ? parsedId : -1;

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


        foreach (var product in products)
        {
            var isFavorite = _context.FavoriteProductList
                .Any(f => f.UserID == userId && f.ProductID == product.ProductID);
            product.isFav = isFavorite;
        }


        var now = DateTime.Now;

        var activePromotions = _context.Promotions
            .Where(p => p.StartDate <= now && p.EndDate >= now)
            .Include(p => p.PromotionDetails)
                .ThenInclude(pd => pd.Product)
            .ToList();

        Console.WriteLine($"Found {activePromotions.Count} active promotions.");

        var productDict = products.ToDictionary(p => p.ProductID);
        Console.WriteLine($"Created product dictionary with {productDict.Count} products.");

        foreach (var promotion in activePromotions)
        {
            Console.WriteLine($"Promotion ID: {promotion.PromotionID} has {promotion.PromotionDetails?.Count ?? 0} promotion details.");

            foreach (var promoDetail in promotion.PromotionDetails!)
            {
                Console.WriteLine($"Checking product ID: {promoDetail.ProductID} with new price: {promoDetail.NewPrice}");

                if (productDict.TryGetValue(promoDetail.ProductID, out var product))
                {
                    product.PromotionPrice = promoDetail.NewPrice;
                    Console.WriteLine($"Updated product ID: {product.ProductID} with PromotionPrice: {product.PromotionPrice}");
                }
                else
                {
                    Console.WriteLine($"Product ID: {promoDetail.ProductID} not found in product dictionary.");
                }
            }
        }

        var productViewModel = new ProductViewModel
        {
            Products = products,
            Promotions = activePromotions,
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
    public IActionResult Detail(int id, int page = 1)
    {

        int pageSize = 5; // Số đánh giá mỗi trang
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }

        

        int userId = int.TryParse(User.FindFirstValue("UserID"), out int parsedId) ? parsedId : -1;

        var product = _context.ProductList
            .Include(p => p.Stocks)
            .Include(p => p.SubCategory)
            .ThenInclude(sc => sc.Category)
            .Include(p => p.Images)
            .FirstOrDefault(p => p.ProductID == id&&p.Stocks.Any(s => s.ExpirationDate > DateTime.Now));

        var user = _context.UserList.ToList();
        foreach (var u in user)
        {
            u.TotalOrders = _context.OrderList.Count(o => o.UserID == u.UserID);
            u.TotalReviews = _context.ReviewList.Count(r => r.UserID == u.UserID);
            u.TotalProducts = _context.OrderList
                .Where(o => o.UserID == u.UserID)
                .SelectMany(o => o.OrderDetails) // nối sang chi tiết đơn hàng
                .Sum(od => od.Quantity);
        }
        // Lấy danh sách đánh giá với phân trang
        var reviews =  _context.ReviewList
            .Where(r => r.ProductID == id)
            .Include(r => r.User) 
            .OrderByDescending(r => r.ReviewDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Tính tổng số đánh giá
        var totalReviews =  _context.ReviewList.Count(r => r.ProductID == id);

        // Tính phân bố số sao
        var ratingDistribution = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            ratingDistribution[i] =  _context.ReviewList
                .Count(r => r.ProductID == id && r.Rating == i);
        }

        // Tính và cập nhật AverageRating
        var averageRating = totalReviews > 0
            ?  _context.ReviewList
                .Where(r => r.ProductID == id)
                .Average(r => (decimal)r.Rating)
            : 0;
        var view = new ProductDetailViewModel
        {
            Product = product,
            Reviews = reviews,
            TotalReviews = totalReviews,
            RatingDistribution = ratingDistribution,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize),
            AverageRating = (double)Math.Round(averageRating, 1), // Làm tròn 1 chữ số thập phân
            
        };

        
        product.isFav = _context.FavoriteProductList
        .Any(p => p.ProductID == id && p.UserID == userId);
        _context.Update(product);
        _context.SaveChanges();
        if (product == null)
        {
            return NotFound();
        }

        

        return View(view);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToFavorites(int productId, string returnUrl = null)
    {
        int userId = int.TryParse(User.FindFirstValue("UserID"), out int parsedId) ? parsedId : -1;
        if (userId == -1)
        {
            return Unauthorized();
        }
        var favorite = await _context.FavoriteProductList
            .FirstOrDefaultAsync(f => f.UserID == userId && f.ProductID == productId);

        if (favorite == null)
        {
            _context.FavoriteProductList.Add(new FavoriteProduct
            {
                UserID = userId,
                ProductID = productId
            });
            await _context.SaveChangesAsync();
        }

        var product = await _context.ProductList.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (product != null)
            product.isFav = true;

        return Ok();
    }
    // Xử lý gửi đánh giá
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview(int productId, int rating, string comment)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized("Vui lòng đăng nhập để gửi đánh giá.");
        }

        // Lấy UserId từ người dùng đang đăng nhập (giả định bạn dùng ASP.NET Identity)
        int userId = int.TryParse(User.FindFirstValue("UserID"), out int parsedId) ? parsedId : -1;

        // Chống spam: Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
        var existingReview = await _context.ReviewList
            .FirstOrDefaultAsync(r => r.UserID == userId && r.ProductID== productId);

        if (existingReview != null)
        {
            return BadRequest("Bạn đã gửi đánh giá cho sản phẩm này rồi.");
        }

        // Chống spam: Kiểm tra độ dài bình luận
        if (string.IsNullOrWhiteSpace(comment) || comment.Length < 10)
        {
            return BadRequest("Bình luận phải có ít nhất 10 ký tự.");
        }

        // Kiểm tra rating hợp lệ
        if (rating < 1 || rating > 5)
        {
            return BadRequest("Số sao phải từ 1 đến 5.");
        }

        var review = new Review
        {
            UserID = userId,
            ProductID = productId,
            Rating = rating,
            Comment = comment,
            ReviewDate = DateTime.Now
        };

        _context.ReviewList.Add(review);
        await _context.SaveChangesAsync();

        // Reload trang sau khi gửi đánh giá
        return RedirectToAction("Detail", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromFavorites(int productId, string returnUrl = null)
    {
        int userId = int.TryParse(User.FindFirstValue("UserID"), out int parsedId) ? parsedId : -1;

        var favorite = await _context.FavoriteProductList
            .FirstOrDefaultAsync(f => f.UserID == userId && f.ProductID == productId);

        if (favorite != null)
        {
            _context.FavoriteProductList.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        var product = await _context.ProductList.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (product != null)
            product.isFav = false;

        return Ok(); // if calling from JavaScript and only expect status
    }
}


