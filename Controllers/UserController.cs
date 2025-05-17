using BachHoaXanh.Data;
using BachHoaXanh.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BachHoaXanh.Helpers;
using BachHoaXanh.ViewModels;
using System.Net;
using Azure;
using System.Linq;

namespace BachHoaXanh.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Register
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new User());
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
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

                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (user != null)
                {
                    _logger.LogInformation($"User found: {user.UserName}");

                    string hashedPassword = MD5Hasher.HashPassword(model.Password);

                    if (user.Password == hashedPassword)
                    {
                        _logger.LogInformation($"User {user.UserName} authenticated successfully.");
                        Console.WriteLine($"User {user.UserID} authenticated successfully.");

                        var claims = new List<Claim> {
                            new Claim("UserID", user.UserID.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(7)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid password attempt for user {model.UserName}.");
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
                else
                {
                    _logger.LogWarning($"User not found: {model.UserName}");
                    ModelState.AddModelError("", "User not found.");
                }
            }

            return View(model);
        }

        // Logout
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Route("thong-tin-tai-khoan")]
        [Authorize]
        public IActionResult Profile()
        {
            _logger.LogInformation("User Claims:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            var userName = User.Identity.Name;
            _logger.LogInformation("User.Identity.Name: {UserName}", userName ?? "null");

            var user = _context.UserList
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng với UserName: {UserName}", userName);
                return NotFound();
            }

            _logger.LogInformation("Tìm thấy người dùng: UserName={UserName}, Email={Email}, Address={Address}, Phone={Phone}",
                user.UserName, user.Email, user.Address, user.Phone);

            var model = new UpdateProfileView
            {
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Phone = user.Phone,
                Addresses = user.Addresses?.ToList() ?? new List<Address>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("thong-tin-tai-khoan")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileView model)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.Name;
                var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserName == userName);

                if (user != null)
                {
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.Address = model.Address;

                    _context.UserList.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Profile");
                }
            } else
            {
                // Ghi log lỗi nếu model không hợp lệ
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
            }

            // Reload addresses if model state is invalid
            var userWithAddresses = await _context.UserList
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            model.Addresses = userWithAddresses?.Addresses?.ToList() ?? new List<Address>();

            return View("Profile", model);
        }

        [HttpGet]
        [Route("them-dia-chi")]
        [Authorize]
        public IActionResult AddAddress()
        {
            return View(new AddressViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("them-dia-chi")]
        [Authorize]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.Name;
                var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserName == userName);

                if (user != null)
                {
                    var address = new Address
                    {
                        UserID = user.UserID,
                        Province = model.Province,
                        District = model.District,
                        Ward = model.Ward,
                        Street = model.Street,
                        IsDefault = model.IsDefault
                    };

                    // If setting as default, unset other default addresses
                    if (model.IsDefault)
                    {
                        var existingAddresses = await _context.Addresses
                            .Where(a => a.UserID == user.UserID && a.IsDefault)
                            .ToListAsync();
                        foreach (var addr in existingAddresses)
                        {
                            addr.IsDefault = false;
                            _context.Addresses.Update(addr);
                        }
                    }

                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Profile");
                }
                ModelState.AddModelError("", "User not found.");
            }

            return View(model);
        }

        [HttpGet]
        [Route("sua-dia-chi/{id}")]
        [Authorize]
        public async Task<IActionResult> EditAddress(int id)
        {
            var userName = User.Identity.Name;
            var address = await _context.Addresses
                .Where(a => a.AddressID == id && a.User.UserName == userName)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                return NotFound();
            }

        [Route("san-pham-yeu-thich")]
        [Authorize]
        public IActionResult Favorite(int page = 1, string search = "", int? danhmuc = null)
        {
            int userId = int.Parse(User.FindFirstValue("UserID"));

            int pageSize = 8;
            var categories = _context.CategoryList.Include(c => c.SubCategories).ToList();
            var productsQuery = _context.ProductList
                .Where(p => _context.FavoriteProductList
                    .Any(f => f.UserID == userId && f.ProductID == p.ProductID))
                .AsQueryable();

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

    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sua-dia-chi/{id}")]
        [Authorize]
        public async Task<IActionResult> EditAddress(int id, AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.Name;
                var address = await _context.Addresses
                    .Where(a => a.AddressID == id && a.User.UserName == userName)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return NotFound();
                }

                address.Province = model.Province;
                address.District = model.District;
                address.Ward = model.Ward;
                address.Street = model.Street;
                address.IsDefault = model.IsDefault;

                // If setting as default, unset other default addresses
                if (model.IsDefault)
                {
                    var existingAddresses = await _context.Addresses
                        .Where(a => a.UserID == address.UserID && a.IsDefault && a.AddressID != id)
                        .ToListAsync();
                    foreach (var addr in existingAddresses)
                    {
                        addr.IsDefault = false;
                        _context.Addresses.Update(addr);
                    }
                }

                _context.Addresses.Update(address);
                await _context.SaveChangesAsync();

                return RedirectToAction("Profile");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("xoa-dia-chi/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userName = User.Identity.Name;
            var address = await _context.Addresses
                .Where(a => a.AddressID == id && a.User.UserName == userName)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }
    }
}