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
            try
            {
                var cartItems = GetCartItems();
                var userN = User.Identity.Name;
                User u = _context.UserList
                    .Include(u => u.Addresses)
                    .AsSplitQuery()
                    .FirstOrDefault(u => u.UserName == userN);

                if (u == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validate and update cart items based on current stock
                var updatedCart = new List<CartItem>();
                bool cartUpdated = false;

                foreach (var item in cartItems)
                {
                    var product = _context.ProductList
                        .Include(p => p.Images)
                        .Include(p => p.Stocks)
                        .FirstOrDefault(p => p.ProductID == item.ProductID);

                    if (product == null)
                    {
                        cartUpdated = true;
                        continue;
                    }

                    int stockQuantity = product.Stocks?.Sum(s => s.Quantity) ?? 0;
                    
                    if (stockQuantity <= 0)
                    {
                        cartUpdated = true;
                        continue;
                    }

                    if (item.Quantity > stockQuantity)
                    {
                        item.Quantity = stockQuantity;
                        cartUpdated = true;
                    }

                    updatedCart.Add(item);
                }

                if (cartUpdated)
                {
                    SaveCartSession(updatedCart);
                    TempData["CartMessage"] = "Giỏ hàng đã được cập nhật do thay đổi trong kho hàng.";
                }

                var model = new CartView
                {
                    CartItems = updatedCart,
                    User = u
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error in Cart Index: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải giỏ hàng.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("them-gio-hang/{productid}", Name = "them-gio-hang")]
        [HttpPost]
        [Authorize]
        public IActionResult AddToCart([FromRoute] int productid, [FromForm] int quantity = 1, [FromForm] string? note = null, [FromForm] int paymentMethodID = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0";
                    return RedirectToAction(nameof(Index));
                }

                var product = _context.ProductList
                    .Include(p => p.Images)
                    .Include(p => p.Stocks)
                    .FirstOrDefault(p => p.ProductID == productid);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction(nameof(Index));
                }

                int stockQuantity = product.Stocks?.Sum(s => s.Quantity) ?? 0;
                if (stockQuantity <= 0)
                {
                    TempData["ErrorMessage"] = "Sản phẩm hiện đã hết hàng.";
                    return RedirectToAction(nameof(Index));
                }

                var cart = GetCartItems();
                var cartItem = cart.FirstOrDefault(p => p.ProductID == productid);

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
                    if (quantity > stockQuantity)
                    {
                        TempData["ErrorMessage"] = $"Không thể thêm số lượng này. Chỉ còn {stockQuantity} sản phẩm trong kho.";
                        return RedirectToAction(nameof(Index));
                    }
                    cart.Add(new CartItem
                    {
                        Quantity = quantity,
                        ProductID = product.ProductID,
                        Note = note ?? "",
                        PaymentMethodID = paymentMethodID,
                        Product = product
                    });
                }

                SaveCartSession(cart);
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in AddToCart: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm vào giỏ hàng";
                return RedirectToAction(nameof(Index));
            }
        }

        [Route("/cap-nhat-gio-hang", Name = "cap-nhat-gio-hang")]
        [HttpPost]
        [Authorize]
        public IActionResult UpdateCart([FromBody] List<CartItem> updatedItems)
        {
            try
            {
                if (updatedItems == null || !updatedItems.Any())
                {
                    return BadRequest("Không có dữ liệu cập nhật");
                }

                var cart = GetCartItems();
                bool hasChanges = false;

                foreach (var item in updatedItems)
                {
                    if (item.Quantity <= 0)
                    {
                        continue;
                    }

                    var cartItem = cart.FirstOrDefault(p => p.ProductID == item.ProductID);
                    if (cartItem == null)
                    {
                        continue;
                    }

                    var product = _context.ProductList
                        .Include(p => p.Stocks)
                        .FirstOrDefault(p => p.ProductID == item.ProductID);

                    if (product == null)
                    {
                        continue;
                    }

                    int stockQuantity = product.Stocks?.Sum(s => s.Quantity) ?? 0;
                    if (item.Quantity > stockQuantity)
                    {
                        return BadRequest($"Số lượng vượt quá tồn kho cho sản phẩm {product.ProductName}. Tối đa: {stockQuantity}");
                    }

                    cartItem.Quantity = item.Quantity;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    SaveCartSession(cart);
                }

                return Json(new { success = true, message = "Giỏ hàng đã được cập nhật", cartCount = GetCartItemCount() });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in UpdateCart: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật giỏ hàng");
            }
        }

        [HttpPost]
        [Route("/xoa-gio-hang")]
        [Authorize]
        public IActionResult RemoveCart([FromBody] int productId)
        {
            try
            {
                if (productId == 0)
                {
                    return BadRequest("Không tìm thấy sản phẩm cần xóa");
                }

                var cart = GetCartItems();
                var cartItem = cart.FirstOrDefault(p => p.ProductID == productId);

                if (cartItem != null)
                {
                    cart.Remove(cartItem);
                    SaveCartSession(cart);
                }

                return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng", cartCount = GetCartItemCount() });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in RemoveCart: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng");
            }
        }

        [HttpGet]
        [Route("/cart/count")]
        public IActionResult GetCartItemCount()
        {
            try
            {
                return Json(GetCartItems().Sum(ci => ci.Quantity));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in GetCartItemCount: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi lấy số lượng giỏ hàng");
            }
        }

        private List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsonCart = session.GetString(CARTKEY);
            var cartItems = jsonCart != null
                ? JsonConvert.DeserializeObject<List<CartItem>>(jsonCart)
                : new List<CartItem>();

            foreach (var item in cartItems)
            {
                item.Product = _context.ProductList
                    .Include(p => p.Images)
                    .Include(p => p.Stocks)
                    .FirstOrDefault(p => p.ProductID == item.ProductID);
            }

            // Remove items with null products
            cartItems = cartItems.Where(item => item.Product != null).ToList();
            return cartItems;
        }

        [HttpPost]
        [Route("cap-nhat-phuong-thuc-thanh-toan")]
        [Authorize]
        public IActionResult UpdatePaymentMethod([FromBody] int paymentMethodID)
        {
            try
            {
                if (paymentMethodID <= 0)
                {
                    return BadRequest("Phương thức thanh toán không hợp lệ");
                }

                var cart = GetCartItems();
                foreach (var cartItem in cart)
                {
                    cartItem.PaymentMethodID = paymentMethodID;
                }

                SaveCartSession(cart);
                return Ok(new { success = true, message = "Đã cập nhật phương thức thanh toán" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in UpdatePaymentMethod: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật phương thức thanh toán");
            }
        }

        [HttpPost]
        [Route("xoa-het-gio-hang")]
        [Authorize]
        public IActionResult ClearAllCart()
        {
            try
            {
                ClearCart();
                return Ok(new { success = true, message = "Đã xóa toàn bộ giỏ hàng" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in ClearAllCart: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi xóa giỏ hàng");
            }
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CARTKEY);
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
            try
            {
                var cartItems = GetCartItems();
                if (!cartItems.Any())
                {
                    return BadRequest("Giỏ hàng trống!");
                }

                var userN = User.Identity.Name;
                if (string.IsNullOrEmpty(userN))
                {
                    return Unauthorized("Vui lòng đăng nhập để tiếp tục.");
                }

                User u = _context.UserList.FirstOrDefault(u => u.UserName == userN);
                if (u == null)
                {
                    return NotFound("Không tìm thấy thông tin người dùng.");
                }

                var address = _context.Addresses
                    .FirstOrDefault(a => a.AddressID == shippingAddressID && a.UserID == u.UserID);
                if (address == null)
                {
                    return BadRequest("Địa chỉ giao hàng không hợp lệ.");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var order = new Order
                        {
                            UserID = u.UserID,
                            TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                            PaymentMethodID = cartItems.FirstOrDefault()?.PaymentMethodID ?? 1,
                            ShippingAddress = $"{address.Street}, {address.Ward}, {address.District}, {address.Province}",
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
                                .Include(p => p.Stocks)
                                .FirstOrDefault(p => p.ProductID == item.ProductID);

                            if (product == null)
                            {
                                throw new Exception($"Không tìm thấy sản phẩm: {item.ProductID}");
                            }

                            var orderDetail = new OrderDetail
                            {
                                OrderID = order.OrderID,
                                ProductID = item.ProductID,
                                Quantity = item.Quantity,
                                UnitPrice = item.Product.Price
                            };
                            _context.OrderDetailList.Add(orderDetail);
                            _context.SaveChanges();

                            var availableStocks = product.Stocks
                                .Where(s => s.ExpirationDate >= DateTime.Now && s.Quantity > 0)
                                .OrderBy(s => s.ExpirationDate)
                                .ToList();

                            int remainingQty = item.Quantity;

                            foreach (var stock in availableStocks)
                            {
                                if (remainingQty <= 0) break;

                                int deduct = Math.Min(stock.Quantity, remainingQty);
                                stock.Quantity -= deduct;
                                stock.UpdatedAt = DateTime.Now;
                                _context.StockProductList.Update(stock);

                                var orderStockDetail = new OrderStockDetail
                                {
                                    OrderDetailID = orderDetail.OrderDetailID,
                                    StockID = stock.StockID,
                                    Quantity = deduct
                                };
                                _context.OrderStockDetailList.Add(orderStockDetail);

                                remainingQty -= deduct;
                            }

                            if (remainingQty > 0)
                            {
                                throw new Exception($"Không đủ hàng trong kho cho sản phẩm: {product.ProductName}");
                            }
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        ClearCart();
                        return Ok(new { success = true, message = "Đơn hàng đã được tạo thành công", orderID = order.OrderID });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in CreateOrder: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}