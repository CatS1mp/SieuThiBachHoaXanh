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
            // Debug: Ghi log tất cả claims của người dùng
            _logger.LogInformation("User Claims:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            // Lấy UserName từ User.Identity.Name
            var userName = User.Identity.Name;
            _logger.LogInformation("User.Identity.Name: {UserName}", userName ?? "null");

            // Truy vấn người dùng từ cơ sở dữ liệu
            var user = _context.UserList.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng với UserName: {UserName}", userName);
                return NotFound();
            }

            // Debug: Ghi log thông tin người dùng tìm được
            _logger.LogInformation("Tìm thấy người dùng: UserName={UserName}, Email={Email}, Address={Address}, Phone={Phone}",
                user.UserName, user.Email, user.Address, user.Phone);

            // Tạo model để truyền sang view
            var model = new UpdateProfileView
            {
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Phone = user.Phone
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
            }

            return View("Profile", model);
        }

    }

}