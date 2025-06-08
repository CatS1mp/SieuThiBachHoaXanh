using System.Security.Cryptography;
using System.Text;
using BachHoaXanh.Data;
using Microsoft.AspNetCore.Mvc;

namespace BachHoaXanh.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet, HttpPost]
        public IActionResult CreateVNPayPayment(int orderId)
        {
            try
            {
                var vnp_TmnCode = _configuration["VNPay:TmnCode"];
                var vnp_HashSecret = _configuration["VNPay:HashSecret"];
                var vnp_Url = _configuration["VNPay:BaseUrl"];
                var vnp_ReturnUrl = _configuration["VNPay:ReturnUrl"];

                if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret) ||
                    string.IsNullOrEmpty(vnp_Url) || string.IsNullOrEmpty(vnp_ReturnUrl))
                {
                    return BadRequest("Cấu hình VNPay không hợp lệ.");
                }

                var order = _context.OrderList.Find(orderId);
                if (order == null || order.OrderStatus != "Pending")
                {
                    return BadRequest("Đơn hàng không hợp lệ.");
                }

                if (order.TotalAmount <= 0)
                {
                    return BadRequest("Số tiền không hợp lệ.");
                }

                var ipAddr = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(ipAddr) || ipAddr == "::1") ipAddr = "127.0.0.1";

                // Tạo vnp_TxnRef duy nhất
                var txnRef = $"{order.OrderID}-{DateTime.UtcNow.AddHours(7).Ticks}";

                var vnp_Params = new SortedDictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", vnp_TmnCode },
                    { "vnp_Amount", ((long)(order.TotalAmount * 100)).ToString() },
                    { "vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_IpAddr", ipAddr },
                    { "vnp_Locale", "vn" },
                    { "vnp_OrderInfo", $"Thanhtoandonhang" },
                    { "vnp_OrderType", "billpayment" },
                    { "vnp_ReturnUrl", vnp_ReturnUrl },
                    { "vnp_TxnRef", txnRef }
                };

                foreach (var param in vnp_Params)
                {
                    Console.WriteLine($"  {param.Key}: {param.Value}");
                }
                // Tạo chữ ký bảo mật
                string signData = string.Join("&", vnp_Params.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                Console.WriteLine("signData: " + signData); // Debug
                string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
                Console.WriteLine("vnp_SecureHash: " + vnp_SecureHash); // Debug
                vnp_Params["vnp_SecureHash"] = vnp_SecureHash;

                // Tạo URL thanh toán
                string paymentUrl = vnp_Url + "?" + string.Join("&", vnp_Params.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                Console.WriteLine("paymentUrl: " + paymentUrl); // Debug

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return StatusCode(500, "Lỗi tạo thanh toán.");
            }
        }


        [HttpGet]
        public IActionResult VNPayReturn()
        {
            try
            {
                Console.WriteLine("VNPayReturn callback được gọi");

                var vnp_HashSecret = _configuration["VNPay:HashSecret"];
                var vnp_Params = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

                if (!vnp_Params.TryGetValue("vnp_SecureHash", out var secureHash))
                {
                    return BadRequest("Thiếu chữ ký.");
                }

                vnp_Params.Remove("vnp_SecureHash");
                vnp_Params.Remove("vnp_SecureHashType");

                var signData = string.Join("&", vnp_Params.OrderBy(k => k.Key)
                    .Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var checkSum = HmacSHA512(vnp_HashSecret, signData);

                Console.WriteLine("========= DEBUG THAM SỐ =========");
                foreach (var param in vnp_Params.OrderBy(p => p.Key))
                {
                    Console.WriteLine($"{param.Key} = {param.Value}");
                }
                Console.WriteLine($"-> signData = {signData}");
                Console.WriteLine($"-> secureHash (tính lại) = {checkSum}");
                Console.WriteLine($"-> secureHash (từ VNPay) = {secureHash}");

                if (!checkSum.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase))
                {
                    return BadRequest("Chữ ký không hợp lệ!");
                }

                var orderId = int.Parse(vnp_Params["vnp_TxnRef"]);
                var responseCode = vnp_Params["vnp_ResponseCode"];
                var order = _context.OrderList.Find(orderId);

                if (order == null)
                {
                    return BadRequest("Đơn hàng không tồn tại!");
                }

                if (responseCode == "00")
                {
                    order.OrderStatus = "Paid";
                    _context.SaveChanges();
                    return View("PaymentSuccess", new { Message = "Thanh toán thành công!", OrderId = order.OrderID });
                }
                else
                {
                    return View("PaymentFailure", new { Message = "Thanh toán thất bại: " + vnp_Params.GetValueOrDefault("vnp_Message") });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return StatusCode(500, "Lỗi xử lý thanh toán.");
            }
        }

        private string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
