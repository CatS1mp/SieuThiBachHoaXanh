using BachHoaXanh.Data;
using BachHoaXanh.Helpers;
using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;

namespace BachHoaXanh.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserList.ToListAsync();
            var products = await _context.ProductList.ToListAsync();
            var orders = await _context.OrderList.ToListAsync();
            var cat = await _context.CategoryList.ToListAsync();
            var subcat = await _context.SubCategoryList.ToListAsync();
            var payment = await _context.PaymentMethodList.ToListAsync();

            var model = new AdminViewModel
            {
                Users = users,
                Products = products,
                Orders = orders,
                Categories = cat,
                SubCategories = subcat,
                PaymentMethods = payment
            };
            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> Users()
        {
            var users = await _context.UserList.ToListAsync();
            return View(users);
        }
        [Authorize]
        [Route("api/user/{username}")]
        [HttpDelete]
        public IActionResult DeleteUser(string username)
        {
            var data = _context.UserList.FirstOrDefault(u => u.UserName == username);
            Console.WriteLine("ID to delete " + username);
            if (data == null)
            {
                return NotFound(new { message = "User not found." });
            }

            try
            {
                _context.UserList.Remove(data);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }
        [Authorize]
        public IActionResult UserAdd()
        {
            var userModel = new User();
            return View(userModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAdd(User model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.UserList.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                    return View(model);
                }

                if (await _context.UserList.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }
                model.Password = MD5Hasher.HashPassword(model.Password);

                _context.UserList.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Users));
            }

            return View(model);
        }
        [Authorize]
        public IActionResult UserEdit(string userName)
        {
            var userModel = _context.UserList.FirstOrDefault(u => u.UserName == userName);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }


        [Authorize]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(User model)
        {
            if (ModelState.IsValid)
            {
                var userModel = await _context.UserList.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (userModel == null)
                {
                    return NotFound();
                }

                if (await _context.UserList.AnyAsync(u => u.UserName == model.UserName && u.UserID != userModel.UserID))
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                    return View(model);
                }

                if (await _context.UserList.AnyAsync(u => u.Email == model.Email && u.UserID != userModel.UserID))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                userModel.UserName = model.UserName;
                userModel.Email = model.Email;
                userModel.FullName = model.FullName;
                userModel.Phone = model.Phone;
                userModel.Address = model.Address;
                userModel.Role = model.Role;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    userModel.Password = MD5Hasher.HashPassword(model.Password);
                }

                _context.UserList.Update(userModel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Users));
            }

            return View(model);
        }
        [Authorize]
        public IActionResult Orders()
        {
            var orders = _context.OrderList.Include(o => o.User)
                                        .Include(o => o.PaymentMethod)
                                        .ToList();
            return View(orders);
        }


        [Authorize]
        public IActionResult OrderDetail(int id)
        {
            var order = _context.OrderList
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.User)
                .Include(o => o.PaymentMethod)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize]

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.OrderList.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize]
        public IActionResult Categories()
        {
            var categories = _context.CategoryList
                .Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    TotalProducts = c.SubCategories.Sum(sc => sc.Products.Count),
                    TotalSubCat = c.SubCategories.Count(),
                    SubCategories = c.SubCategories.Select(sc => new SubCategoryViewModel
                    {
                        SubCategoryID = sc.SubCategoryID,
                        SubCategoryName = sc.SubCategoryName,
                        TotalProducts = sc.Products.Count
                    }).ToList()
                }).ToList();

            return View(categories);
        }



        [Authorize]
        [HttpGet]
        public IActionResult GetSubCategories(int id)
        {
            var subcategories = _context.SubCategoryList
                .Where(sc => sc.Category.CategoryID == id)
                .Select(sc => new SubCategoryViewModel
                {
                    SubCategoryID = sc.SubCategoryID,
                    SubCategoryName = sc.SubCategoryName,
                    TotalProducts = sc.Products.Count
                }).ToList();

            return Json(subcategories);
        }
        [Authorize]
        [HttpGet]
        public IActionResult CategoryAdd()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult CategoryAdd(CategoryCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    CategoryName = model.CategoryName
                };

                _context.CategoryList.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult CategoryEdit(int id)
        {
            var category = _context.CategoryList.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound();

            return View(category);
        }
        [Authorize]
        [HttpPost]
        public IActionResult CategoryEdit(Category model)
        {
            if (ModelState.IsValid)
            {
                var category = _context.CategoryList.FirstOrDefault(c => c.CategoryID == model.CategoryID);
                if (category != null)
                {
                    category.CategoryName = model.CategoryName;
                    _context.SaveChanges();
                    return RedirectToAction("Categories");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy danh mục cần sửa.");
                }
            }

            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            try
            {
                var category = await _context.CategoryList.FindAsync(id);

                if (category == null)
                {
                    return NotFound();
                }

                _context.CategoryList.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category deleted successfully!";
                return RedirectToAction("Categories"); // Replace with your action that lists categories
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category: " + ex.Message;
                return RedirectToAction("Categories"); // Replace with your action that lists categories
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult SubCategoryAdd()
        {
            ViewBag.CategoryList = new SelectList(_context.CategoryList, "CategoryID", "CategoryName");
            return View(new SubCategory());
        }

        [Authorize]
        [HttpPost]
        public IActionResult SubCategoryAdd(SubCategory model)
        {
            Console.WriteLine("2");
            _context.SubCategoryList.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Categories");
        }
        [Authorize]
        [HttpGet]
        public IActionResult SubCategoryEdit(int id)
        {
            var subCategory = _context.SubCategoryList
                .FirstOrDefault(sc => sc.SubCategoryID == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            var categories = _context.CategoryList.ToList();
            ViewBag.CategoryList = new SelectList(categories, "CategoryID", "CategoryName", subCategory.CategoryID);

            return View(subCategory);
        }
        [Authorize]
        [HttpPost]
        public IActionResult SubCategoryEdit(SubCategory model)
        {
            var existingSubCategory = _context.SubCategoryList
                .FirstOrDefault(sc => sc.SubCategoryID == model.SubCategoryID);

            if (existingSubCategory != null)
            {
                existingSubCategory.SubCategoryName = model.SubCategoryName;
                existingSubCategory.CategoryID = model.CategoryID;

                _context.SaveChanges();

                return RedirectToAction("Categories");
            }
            else
            {
                return NotFound("Danh mục con không tồn tại.");
            }

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> subCategoryDelete(int id)
        {
            try
            {
                var subCategory = await _context.SubCategoryList.FindAsync(id);

                if (subCategory == null)
                {
                    return NotFound();
                }

                _context.SubCategoryList.Remove(subCategory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category deleted successfully!";
                return RedirectToAction("Categories"); // Replace with your action that lists categories
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category: " + ex.Message;
                return RedirectToAction("Categories"); // Replace with your action that lists categories
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult Products()
        {
            return View();
        }

        [Authorize]
        [Route("api/products")]
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _context.ProductList
                .Include(p => p.Stocks)
                .Include(p => p.Images)
                .Include(p => p.SubCategory)
                .Select(p => new
                {
                    ProductImageUrl = p.Images.FirstOrDefault(img => img.IsMainImage) != null
                        ? $"/images/prods/{p.Images.FirstOrDefault(img => img.IsMainImage).ImagePath}"
                        : "/images/default/default-image.jpg",
                    ProductName = p.ProductName,
                    SubCategoryName = p.SubCategory != null ? p.SubCategory.SubCategoryName : "Không xác định",
                    Stock = p.StockQuantity,
                    Price = p.Price.ToString("N0") + " VNĐ",
                    Status = p.Status == ProductStatus.KinhDoanh ? "Kinh doanh" :
                             p.Status == ProductStatus.TamHetHang ? "Tạm hết hàng" :
                             p.Status == ProductStatus.NgungKinhDoanh ? "Ngừng kinh doanh" : "Không xác định",
            ProductID = p.ProductID,
                    IsExpired = p.Stocks != null && p.Stocks.Any() && p.Stocks.All(s => s.ExpirationDate < DateTime.Now),
                    IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= 5 // Threshold for low stock
                })
                .ToList();

            return Json(new { data = products });
        }


        [HttpGet]
        public async Task<IActionResult> ProductCreate()
        {
            var subCategories = await _context.SubCategoryList
                .Select(s => new { s.SubCategoryID, s.SubCategoryName })
                .ToListAsync();

            // Nếu có danh sách SubCategories, đặt giá trị mặc định là mục đầu tiên
            var defaultSubCategoryId = subCategories.FirstOrDefault()?.SubCategoryID;

            ViewBag.SubCategoryList = new SelectList(subCategories, "SubCategoryID", "SubCategoryName", defaultSubCategoryId);

            // Khởi tạo sản phẩm với giá trị SubCategoryID mặc định (nếu có)
            var product = new Product
            {
                SubCategoryID = defaultSubCategoryId
            };

            return View(product);
        }

        [Route("api/products/{id}")]
        [HttpDelete]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.ProductList.FirstOrDefault(u => u.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.ProductList.Remove(product);
            _context.SaveChanges();

            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(Product product, List<IFormFile> imageFiles, List<int> newStockQuantities, List<DateTime> newStockExpirationDates)
        {
            product.CreatedAt = DateTime.Now;
            product.Status = ProductStatus.KinhDoanh;
            _context.Add(product);
            await _context.SaveChangesAsync();

            if (product.ProductID == 0)
            {
                ModelState.AddModelError("", "Failed to save the product.");
                return View(product);
            }
            // Handle stock entries
            for (int i = 0; i < newStockQuantities.Count; i++)
            {
                if (newStockQuantities[i] > 0)
                {
                    var stock = new StockProduct
                    {
                        ProductID = product.ProductID,
                        Quantity = newStockQuantities[i],
                        ExpirationDate = newStockExpirationDates.ElementAtOrDefault(i),
                        CreatedAt = DateTime.Now
                    };
                    _context.Add(stock);
                }
            }
            string imagePath = Path.Combine(_environment.WebRootPath, "images","prods");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            // Handle image files
            foreach (var imageFile in imageFiles)
            {
                string fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(imagePath, fileName);

                // Handle duplicate file names
                if (System.IO.File.Exists(filePath))
                {
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                    filePath = Path.Combine(imagePath, fileName);
                }

                // Save the image file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Add ProductImage record
                var productImage = new ProductImage
                {
                    ProductID = product.ProductID,
                    ImagePath = fileName,
                    IsMainImage = imageFiles.IndexOf(imageFile) == 0  // First image is the main image
                };

                _context.Add(productImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        [HttpDelete]
        [Route("api/images/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImageList.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            // Optionally delete the file from the server
            var filePath = Path.Combine(_environment.WebRootPath, "images", image.ImagePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImageList.Remove(image);
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }

        // Sửa sản phẩm - GET
        [HttpGet]
        public async Task<IActionResult> ProductEdit(int id)
        {
            var product = await _context.ProductList
                .Include(p => p.Stocks)
                .Include(p => p.Images)
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.SubCategoryList = new SelectList(_context.SubCategoryList, "SubCategoryID", "SubCategoryName", product.SubCategoryID);
            return View(product);
        }
        // POST: ProductEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(int id, Product product, IFormFile? imageFile, int? newStockQuantities, DateTime? newStockExpirationDates, int? mainImage)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingProduct = await _context.ProductList
                    .Include(p => p.Images)
                    .Include(p => p.Stocks)
                    .FirstOrDefaultAsync(p => p.ProductID == id);

                if (existingProduct == null)
                {
                    return NotFound();
                }
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.SubCategoryID = product.SubCategoryID;
                
                existingProduct.UpdatedAt = DateTime.Now;

                if (newStockQuantities.HasValue && newStockQuantities > 0)
                {
                    if(_context.StockProductList.Any(s => s.ExpirationDate == newStockExpirationDates))
                    {
                        var newStock = await _context.StockProductList
                            .Where(s => s.ExpirationDate == newStockExpirationDates)
                            .FirstOrDefaultAsync();
                        newStock.Quantity += newStockQuantities.GetValueOrDefault();
                        _context.Update(newStock);

                    } else {
                        var newStock = new StockProduct
                        {
                            ProductID = existingProduct.ProductID,
                            Quantity = newStockQuantities.Value,
                            ExpirationDate = newStockExpirationDates ?? DateTime.MaxValue,
                            CreatedAt = DateTime.Now
                        };
                        _context.StockProductList.Add(newStock);

                    }

                    if (existingProduct.Status == ProductStatus.TamHetHang)
                    {
                        existingProduct.Status = ProductStatus.KinhDoanh;
                        _context.Update(existingProduct); // Cập nhật đối tượng trong DbContext
                    }
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var imagePath = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }
                    var filePath = Path.Combine(imagePath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                        filePath = Path.Combine(imagePath, fileName);
                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    var newImage = new ProductImage
                    {
                        ProductID = existingProduct.ProductID,
                        ImagePath = fileName,
                        IsMainImage = false // Will be set below if needed
                    };
                    _context.ProductImageList.Add(newImage);
                    await _context.SaveChangesAsync(); // Save to get ImageID

                }

                // Update main image logic
                var allImages = await _context.ProductImageList
                    .Where(i => i.ProductID == existingProduct.ProductID)
                    .ToListAsync();

                // If user selected a main image, update accordingly
                if (mainImage.HasValue)
                {
                    Console.WriteLine($"MainImage: {mainImage.Value}");
                    foreach (var image in allImages)
                    {
                        if (image.ImageID == mainImage.Value)
                        {
                            image.IsMainImage = true;
                        }
                        else
                        {
                            image.IsMainImage = false;
                        }
                    }
                }
                else
                {
                    // If no selection, ensure there's a main image
                    var hasMainImage = allImages.Any(i => i.IsMainImage);
                    if (!hasMainImage)
                    {
                        if (allImages.Count == 1)
                        {
                            allImages[0].IsMainImage = true;
                        }
                        else
                        {
                            allImages.Last().IsMainImage = true; // Default to newest
                        }
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction("Products");
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm.");
                var productWithDetails = await _context.ProductList
                    .Include(p => p.Stocks)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.ProductID == id);
                if (productWithDetails != null)
                {
                    product.Stocks = productWithDetails.Stocks ?? new List<StockProduct>();
                    product.Images = productWithDetails.Images ?? new List<ProductImage>();
                }
                ViewBag.SubCategoryList = new SelectList(_context.SubCategoryList, "SubCategoryID", "SubCategoryName", product.SubCategoryID);
                return View(product);
            }
        }
        [HttpPut]
        [Route("api/stocks/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] StockProduct stock)
        {
            if (id != stock.StockID)
            {
                return BadRequest();
            }

            var existingStock = await _context.StockProductList.FindAsync(id);
            if (existingStock == null)
            {
                return NotFound();
            }

            existingStock.Quantity = stock.Quantity;
            existingStock.ExpirationDate = stock.ExpirationDate;
            existingStock.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Route("api/stocks/{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _context.StockProductList.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            _context.StockProductList.Remove(stock);
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public IActionResult Auth(int id)
        {
            var history = _context.FaceAuthHistory
            .Include(a => a.User)
            .ToList();
                return View(history);
        }
        public IActionResult FaceAuthDetail(int id)
        {
            var auth = _context.FaceAuthHistory
                .Include(a => a.User)
                .FirstOrDefault(a => a.Id == id);

            if (auth == null)
            {
                return NotFound();
            }

            return View(auth);
        }
    }

    
}
