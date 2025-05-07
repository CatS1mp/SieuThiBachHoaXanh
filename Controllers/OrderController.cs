using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BachHoaXanh.Data;
using Microsoft.EntityFrameworkCore;

namespace BachHoaXanh.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("lich-su-mua-hang")]
        [Authorize]
        public IActionResult Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = _context.OrderList
                .Where(o => o.UserID.ToString() == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        [Route("don-hang/{id}")]
        [Authorize]
        public IActionResult Detail(int id)
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

        [HttpPost]
        [Authorize]
        public IActionResult CancelOrder(int id)
        {
            var order = _context.OrderList.FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return Json(new { success = false });
            }

            order.OrderStatus = "Cancelled";
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
