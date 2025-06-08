using BachHoaXanh.Data;
using BachHoaXanh.Models;
using BachHoaXanh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BachHoaXanh.Controllers
{
    public class CartController : Controller
    {
        public const string CARTKEY = "cart";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("gio-hang")]
        [Authorize]
        public IActionResult Index()
        {
            var cartItems = GetCartItems();
            var userN = User.Identity.Name;
            User u = _context.UserList
                .Include(u => u.Addresses)
                .AsSplitQuery() // Optimize query performance
                .FirstOrDefault(u => u.UserName == userN);
            var model = new CartView
            {
                CartItems = cartItems,
                User = u
            };
            return View(model);
        }

        [Route("them-gio-hang/{productid}", Name = "them-gio-hang")]
        public IActionResult AddToCart([FromRoute] int productid, [FromForm] int quantity, [FromForm] string? note, [FromForm] int paymentMethodID)
        {
            var product = _context.ProductList
                .Include(p => p.Images)
                .Include(p => p.Stocks)
                .FirstOrDefault(p => p.ProductID == productid);

            if (product == null)
                return NotFound("Không có sản phẩm");

            // Calculate stock quantity
            int stockQuantity = product.Stocks?.Sum(s => s.Quantity) ?? 0;
            if (stockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm hiện đã hết hàng.";
                return RedirectToAction(nameof(Index));
            }


            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(p => p.ProductID == productid);

            paymentMethodID = paymentMethodID != 0 ? paymentMethodID : 1;

            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > stockQuantity)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm số lượng này. Chỉ còn {stockQuantity} sản phẩm trong kho.";
                    return RedirectToAction(nameof(Index));
                }
                cartItem.Quantity += quantity;
                cartItem.Note = note ?? cartItem.Note;
                cartItem.PaymentMethodID = paymentMethodID;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Quantity = quantity,
                    ProductID = product.ProductID,
                    Note = note ?? "",
                    PaymentMethodID = paymentMethodID
                });
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        [Route("/cap-nhat-gio-hang", Name = "cap-nhat-gio-hang")]
        [HttpPost]
        public IActionResult UpdateCart([FromBody] List<CartItem> updatedItems)
        {
            var cart = GetCartItems();

            foreach (var item in updatedItems)
            {
                var cartItem = cart.FirstOrDefault(p => p.ProductID == item.ProductID);

                if (cartItem != null)
                {
                    var product = _context.ProductList
                        .Include(p => p.Stocks)
                        .FirstOrDefault(p => p.ProductID == item.ProductID);
                    if (product != null && item.Quantity > product.StockQuantity)
                    {
                        return BadRequest($"Số lượng vượt quá tồn kho. Tối đa: {product.StockQuantity}");
                    }
                    cartItem.Quantity = item.Quantity;
                    cartItem.Note = item.Note ?? cartItem.Note;
                    cartItem.PaymentMethodID = item.PaymentMethodID != 0 ? item.PaymentMethodID : cartItem.PaymentMethodID;
                }
            }

            SaveCartSession(cart);
            return Ok();
        }

        [HttpPost]
        [Route("/xoa-gio-hang")]
        public IActionResult RemoveCart([FromBody] int productId)
        {
            if (productId == 0)
            {
                return BadRequest("Product ID is required.");
            }

            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(p => p.ProductID == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCartSession(cart);
            }

            return Json(new { success = true, cartCount = cart.Count() });
        }

        public int GetCartItemCount()
        {
            var cartItems = GetCartItems();
            return cartItems.Sum(ci => ci.Quantity);
        }

        public List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsonCart = session.GetString(CARTKEY);
            var cartItems = jsonCart != null
                ? JsonConvert.DeserializeObject<List<CartItem>>(jsonCart)
                : new List<CartItem>();

            // Always reload Product and Images from DB for each cart item
            foreach (var item in cartItems)
            {
                item.Product = _context.ProductList
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.ProductID == item.ProductID);
                item.ProductImages = item.Product?.Images ?? new List<ProductImage>();
            }
            return cartItems;
        }

        [HttpPost]
        [Route("cap-nhat-phuong-thuc-thanh-toan")]
        public IActionResult UpdatePaymentMethod([FromBody] int paymentMethodID)
        {
            if (paymentMethodID == 0)
            {
                return BadRequest("Payment Method ID is required.");
            }

            var cart = GetCartItems();
            foreach (var cartItem in cart)
            {
                cartItem.PaymentMethodID = paymentMethodID;
            }

            SaveCartSession(cart);
            return Ok(new { message = "Payment method updated successfully." });
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CARTKEY);
        }

        [HttpPost]
        [Route("xoa-het-gio-hang")]
        public IActionResult ClearAllCart()
        {
            try
            {
                ClearCart();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error clearing cart: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        private void SaveCartSession(List<CartItem> cartItems)
        {
            var session = HttpContext.Session;
            string jsonCart = JsonConvert.SerializeObject(cartItems);
            session.SetString(CARTKEY, jsonCart);
        }

        [HttpPost]
        [Route("tao-don-hang")]
        [Authorize]
        public IActionResult CreateOrder([FromForm] int shippingAddressID, [FromForm] string note)
        {
            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                return BadRequest("Giỏ hàng trống!");
            }

            var userN = User.Identity.Name;
            if (userN == null)
            {
                return Unauthorized("Người dùng chưa đăng nhập.");
            }

            User u = _context.UserList.FirstOrDefault(u => u.UserName == userN);
            if (u == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Lấy địa chỉ từ shippingAddressID
            var address = _context.Addresses
                .FirstOrDefault(a => a.AddressID == shippingAddressID && a.UserID == u.UserID);
            if (address == null)
            {
                return BadRequest("Địa chỉ giao hàng không hợp lệ.");
            }

            var paymentMethodID = cartItems.FirstOrDefault()?.PaymentMethodID ?? 1;
            if (paymentMethodID == 0)
            {
                return BadRequest("Phương thức thanh toán không hợp lệ.");
            }

            // Tạo chuỗi địa chỉ đầy đủ từ Address
            string fullAddress = $"{address.Street}, {address.Ward}, {address.District}, {address.Province}";

            var order = new Order
            {
                UserID = u.UserID,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                PaymentMethodID = paymentMethodID,
                ShippingAddress = fullAddress,
                CreatedAt = DateTime.Now,
                OrderStatus = "Pending",
                Note = note,
                CanCancel = true
            };

            _context.OrderList.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var product = _context.ProductList
                    .Include(u => u.Stocks)
                    .FirstOrDefault(u => u.ProductID == item.ProductID);

                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                _context.OrderDetailList.Add(orderDetail);
                _context.SaveChanges();

                var sortedStocks = product.Stocks
                    .Where(s => s.ExpirationDate >= DateTime.Now && s.Quantity > 0)
                    .OrderBy(s => s.ExpirationDate)
                    .ToList();
                int remainingQty = item.Quantity;
                Console.WriteLine($"ProductID: {product.ProductID}, sortedStocks.Count: {sortedStocks.Count}, initialQty: {item.Quantity}");

                foreach (var stock in sortedStocks)
                {
                    if (remainingQty <= 0)
                        break;

                    int deduct = Math.Min(stock.Quantity, remainingQty);

                    // Trừ kho
                    stock.Quantity -= deduct;
                    stock.UpdatedAt = DateTime.Now;
                    _context.StockProductList.Update(stock);

                    // Ghi nhận chi tiết lô hàng đã dùng
                    var orderStockDetail = new OrderStockDetail
                    {
                        OrderDetailID = orderDetail.OrderDetailID,
                        StockID = stock.StockID,
                        Quantity = deduct
                    };
                    _context.OrderStockDetailList.Add(orderStockDetail);

                    remainingQty -= deduct;
                    Console.WriteLine($"quantity: {remainingQty}");
                }
                // Nếu vẫn còn số lượng cần trừ mà hết stock
                if (remainingQty > 0)
                {
                    return BadRequest($"Không đủ hàng trong kho cho sản phẩm: {product.ProductName}");
                }
            }

            _context.SaveChanges();

            ClearCart();

            return Ok(new { message = "Đơn hàng đã được tạo thành công", orderID = order.OrderID });
        }
    }
}