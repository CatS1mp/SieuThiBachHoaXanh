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
            User u = _context.UserList.FirstOrDefault(u => u.UserName == userN);
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
                .FirstOrDefault(p => p.ProductID == productid);

            if (product == null)
                return NotFound("Không có sản phẩm");

            quantity = Math.Clamp(quantity, 1, product.StockQuantity);

            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(p => p.ProductID == productid);

            paymentMethodID = paymentMethodID != 0 ? paymentMethodID : 1;

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.Note = note ?? cartItem.Note; 
                cartItem.PaymentMethodID = paymentMethodID;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Quantity = quantity,
                    Product = product,
                    ProductID = product.ProductID,
                    ProductImages = product.Images,
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
                    cartItem.Quantity = item.Quantity;
                    cartItem.Note = item.Note ?? cartItem.Note; // Update note
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

        // Retrieve cart items from session
        public List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsonCart = session.GetString(CARTKEY);
            return jsonCart != null
                ? JsonConvert.DeserializeObject<List<CartItem>>(jsonCart)
                : new List<CartItem>();
        }
        [HttpPost]
        [Route("cap-nhat-phuong-thuc-thanh-toan")]
        public IActionResult UpdatePaymentMethod([FromBody] int paymentMethodID)
        {
            Console.WriteLine("Payement id request update is: " + paymentMethodID);
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

        // Clear the cart
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
        public IActionResult CreateOrder([FromForm] string note)
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

            var paymentMethodID = cartItems.FirstOrDefault()?.PaymentMethodID ?? 1;
            if (paymentMethodID == 0)
            {
                return BadRequest("Phương thức thanh toán không hợp lệ.");
            }

            var order = new Order
            {
                UserID = u.UserID,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                PaymentMethodID = paymentMethodID,
                ShippingAddress = u.Address,
                CreatedAt = DateTime.Now,
                OrderStatus = "Pending",
                Note = note
            };

            _context.OrderList.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };

                _context.OrderDetailList.Add(orderDetail);
            }

            _context.SaveChanges();

            ClearCart();

            return Ok(new { message = "Đơn hàng đã được tạo thành công", orderID = order.OrderID });
        }




    }

}
