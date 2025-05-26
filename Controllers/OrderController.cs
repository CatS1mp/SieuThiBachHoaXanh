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

            // Tính điểm và cập nhật xếp hạng cho người dùng
            UpdateUserPointsAndRank(int.Parse(userId));

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

            // Cập nhật lại điểm và xếp hạng sau khi hủy đơn
            UpdateUserPointsAndRank(order.UserID);

            return Json(new { success = true });
        }

        private void UpdateUserPointsAndRank(int userId)
        {
            // Lấy tất cả đơn hàng của người dùng, chỉ tính khi OrderStatus là "Completed"
            var orders = _context.OrderList
                .Where(o => o.UserID == userId && o.OrderStatus == "Completed")
                .ToList();

            // Tính tổng số tiền từ các đơn hàng
            decimal totalSpent = orders.Sum(o => o.TotalAmount);

            // Tính điểm: 100,000 VNĐ = 10 điểm
            decimal points = (totalSpent / 100000) * 10;

            // Cập nhật xếp hạng dựa trên điểm
            string rank = "Chưa xếp hạng";
            if (points >= 10000)
                rank = "Kim cương";
            else if (points >= 1000)
                rank = "Vàng";
            else if (points >= 500)
                rank = "Bạc";
            else if (points >= 100)
                rank = "Đồng";

            // Cập nhật điểm và xếp hạng vào bảng Users
            var user = _context.UserList.FirstOrDefault(u => u.UserID == userId);
            if (user != null)
            {
                user.Points = points;
                user.Rank = rank;
                _context.SaveChanges();
            }
        }
    }
}