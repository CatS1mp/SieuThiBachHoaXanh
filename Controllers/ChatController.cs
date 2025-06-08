using BachHoaXanh.Data;
using BachHoaXanh.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Markdig;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.IO;
using System.Text.Json;

namespace BachHoaXanh.Controllers
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatController(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> GeminiChat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserInput))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập câu hỏi" });
        }

            try
        {
                // Nâng cấp prompt để yêu cầu Gemini trả về JSON nếu ý định mua hàng
                var systemPrompt = @"@""
                    Bạn là trợ lý AI của Bách Hóa Xanh — một chuỗi cửa hàng bán lẻ chuyên về thực phẩm, đồ uống, hàng gia dụng, điện tử và các sản phẩm tiêu dùng thiết yếu. Hãy trả lời người dùng một cách thân thiện, chuyên nghiệp và sử dụng thông tin từ cơ sở dữ liệu có sẵn.

                    ### 📌 Định dạng câu trả lời:
                    - **Tên sản phẩm**: In đậm.
                    - Danh sách sản phẩm: Dùng dấu * hoặc -.
                    - Giá sản phẩm: In *nghiêng*.
                    - Tồn kho và hạn sử dụng: Hiển thị nếu có.
                    - Danh mục và danh mục phụ: Nêu rõ nếu có.
                    - Hình ảnh sản phẩm: Hiển thị nếu có, dùng thẻ `<img src='url' alt='tên sản phẩm'>`.
                    - Liên kết chi tiết sản phẩm: Dùng định dạng [Xem chi tiết sản phẩm](url).

                    ### 🛒 Nhận diện ý định mua hàng:
                    Nếu phát hiện người dùng có ý định mua, đặt hàng, thêm sản phẩm vào giỏ hàng hoặc sử dụng các cụm từ như:
                    - ""mua"", ""mua ngay"", ""mình muốn mua"", ""thêm vào giỏ"", ""đặt hàng"", ""cho vào giỏ"", ""chốt đơn"", ""lấy cái này"", ...
                    hoặc các câu tương tự, và bạn xác định được sản phẩm liên quan, thì:

                    1. Trả về một dòng JSON trên dòng đầu tiên (KHÔNG hiển thị với người dùng), theo định dạng:
                    ```json
                    { ""action"": ""add_to_cart"", ""product_name"": ""Tên sản phẩm"" }
                    ```
                    2. Sau đó tiếp tục trả lời người dùng bằng markdown như bình thường.

                    ⚠️ Tuyệt đối không hiển thị dòng JSON này cho người dùng. Nó chỉ dành cho hệ thống backend xử lý hành động.

                    Nếu không chắc chắn sản phẩm, có thể không trả JSON và chỉ trả lời bình thường.

                    🔐 Yêu cầu thông tin cá nhân:
                    Nếu người dùng yêu cầu thông tin như đơn hàng, giỏ hàng, sản phẩm yêu thích hoặc lịch sử mua hàng:

                    Hãy kiểm tra xem người dùng đã đăng nhập chưa (dựa trên thông tin hệ thống cung cấp).

                    Nếu chưa đăng nhập: Yêu cầu họ đăng nhập.

                    Nếu không tìm thấy dữ liệu: Thông báo rằng thông tin không khả dụng và gợi ý họ liên hệ bộ phận hỗ trợ.

                    ⚠️ Không được tiết lộ thông tin nhạy cảm như mật khẩu, địa chỉ email, hoặc thông tin hệ thống.";

                // Lấy dữ liệu sản phẩm
                var products = _context.ProductList
                    .Join(_context.SubCategoryList,
                        p => p.SubCategoryID,
                        sc => sc.SubCategoryID,
                        (p, sc) => new { p, sc })
                    .Join(_context.CategoryList,
                        psc => psc.sc.CategoryID,
                        c => c.CategoryID,
                        (psc, c) => new
                        {
                            psc.p.ProductID,
                            psc.p.ProductName,
                            psc.p.Description,
                            psc.p.Price,
                            CategoryName = c.CategoryName,
                            SubCategoryName = psc.sc.SubCategoryName,
                            psc.p.Status
                        })
                    .Where(p => p.Status != ProductStatus.NgungKinhDoanh)
                    .ToList();

                var stock = _context.StockProductList
                    .Select(s => new { s.ProductID, s.Quantity, s.ExpirationDate })
                    .ToList();

                var images = _context.ProductImageList
                    .Where(i => i.IsMainImage)
                    .Select(i => new { i.ProductID, i.ImagePath })
                    .ToList();

                var productInfo = new StringBuilder();
                foreach (var product in products)
                {
                    var stockInfo = stock.FirstOrDefault(s => s.ProductID == product.ProductID);
                    var imageInfo = images.FirstOrDefault(i => i.ProductID == product.ProductID);
                    string imageUrl = null;
                    if (imageInfo != null && !string.IsNullOrEmpty(imageInfo.ImagePath))
                    {
                        var fileName = System.IO.Path.GetFileName(imageInfo.ImagePath);
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images","prods", fileName);

                        if (System.IO.File.Exists(physicalPath))
                        {
                            imageUrl = Url.Content("~/images/prods/" + fileName);
                            Console.WriteLine($"path: {imageUrl}");

                        }
                    }
                    string productUrl = $"{Request.Scheme}://{Request.Host}/san-pham/{product.ProductID}";

                    productInfo.AppendLine($"### {product.ProductName}");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        productInfo.AppendLine($"![{product.ProductName}]({imageUrl})");
                    }
                    else
                    {
                        productInfo.AppendLine("- Ảnh sản phẩm: Không có ảnh chính");
                    }
                    productInfo.AppendLine($"- **Danh mục**: {product.CategoryName} > {product.SubCategoryName}");
                    productInfo.AppendLine($"- **Mô tả**: {product.Description ?? "Không có mô tả"}");
                    productInfo.AppendLine($"- **Giá**: *{product.Price:N0}đ*");
                    if (stockInfo != null)
                    {
                        productInfo.AppendLine($"- **Tồn kho**: {stockInfo.Quantity}");
                        productInfo.AppendLine($"- **Hạn sử dụng**: {stockInfo.ExpirationDate:dd/MM/yyyy}");
                    }
                    productInfo.AppendLine($"- [Xem chi tiết sản phẩm]({productUrl})");
                    productInfo.AppendLine();
                }

                var categoryInfo = string.Join(", ", _context.CategoryList.Select(c => c.CategoryName));
                var subcategoryInfo = string.Join(", ", _context.SubCategoryList.Select(sc => sc.SubCategoryName));

                // Thông tin người dùng
                string userInfo = "";
                var user = _httpContextAccessor.HttpContext.User;
                if (user.Identity.IsAuthenticated)
                {
                    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        var favorites = _context.FavoriteProductList
                            .Where(f => f.UserID == userId)
                            .Join(_context.ProductList,
                                f => f.ProductID,
                                p => p.ProductID,
                                (f, p) => p.ProductName)
                    .ToList();

                        var orders = _context.OrderList
                            .Where(o => o.UserID == userId)
                            .Select(o => new { o.OrderID, o.OrderStatus, o.TotalAmount, o.CreatedAt })
                    .ToList();

                        userInfo = $"\n**Thông tin người dùng**:\n";
                        if (favorites.Any())
                        {
                            userInfo += "- Sản phẩm yêu thích: " + string.Join(", ", favorites) + "\n";
                        }
                        if (orders.Any())
                        {
                            userInfo += "- Đơn hàng:\n";
                            foreach (var order in orders)
                            {
                                userInfo += $"  - Đơn hàng ID: {order.OrderID}: {order.OrderStatus}, Tổng: *{order.TotalAmount:N0}đ*, Ngày tạo: {order.CreatedAt:dd/MM/yyyy}\n";
                            }
                        }
                    }
                }

                var fullContext = $@"**Danh mục sản phẩm**: {categoryInfo}
                **Danh mục phụ**: {subcategoryInfo}
                **Sản phẩm**:
                {productInfo}{userInfo}";

                                var fullPrompt = $@"{systemPrompt}

                **Thông tin cơ sở dữ liệu**:
                {fullContext}

                **Câu hỏi người dùng**: {request.UserInput}";

                // Gọi Gemini API
                var apiKey = _configuration["GeminiApi:ApiKey"];
                var endpoint = _configuration["GeminiApi:ApiEndpoint"];
                var client = _httpClientFactory.CreateClient();

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    }
                };

                var requestUri = $"{endpoint}?key={apiKey}";
                var response = await client.PostAsJsonAsync(requestUri, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { success = false, message = $"Gemini API error: {errorContent}" });
                }

                var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiGenerativeResponse>();
                var generatedText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "Không có phản hồi từ Gemini API.";

                var session = _httpContextAccessor.HttpContext.Session;

                // Lấy dòng JSON đầu tiên thực sự (bỏ qua dòng trắng)
                // Lấy dòng JSON đầu tiên thực sự (bỏ qua dòng trắng)
                // Tách dòng từ đoạn generatedText
                var linesAll = generatedText.Split('\n').ToList();

                // Tìm và lấy toàn bộ JSON trong khối ```json ... ```
                string jsonBlock = "";
                bool insideJsonBlock = false;

                foreach (var line in linesAll)
                {
                    if (line.Trim().StartsWith("```json"))
                    {
                        insideJsonBlock = true;
                        continue;
                    }
                    if (insideJsonBlock)
                    {
                        if (line.Trim().StartsWith("```"))
                            break;
                        jsonBlock += line + "\n";
                    }
                }

                GeminiAction actionObj = null;

                if (!string.IsNullOrWhiteSpace(jsonBlock))
                {
                    Console.WriteLine($"[DEBUG] Extracted JSON block:\n{jsonBlock}");
                    try
                    {
                        actionObj = JsonSerializer.Deserialize<GeminiAction>(jsonBlock);
                        Console.WriteLine("[DEBUG] Parsed JSON block thành công.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Không thể parse JSON: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("[DEBUG] Không tìm thấy khối JSON nào.");

                    // In vài dòng đầu tiên của generatedText để kiểm tra nội dung
                    var previewLines = linesAll.Take(5).ToList();
                    Console.WriteLine("[DEBUG] 5 dòng đầu của generatedText:");
                    foreach (var line in previewLines)
                    {
                        Console.WriteLine($"--> {line}");
                    }
                }

                // Xoá dòng markdown ```json...``` và ``` kết thúc khỏi generatedText
                var lines = generatedText.Split('\n').ToList();
                bool skipBlock = false;
                for (int i = 0; i < lines.Count;)
                {
                    var trimmed = lines[i].Trim();
                    if (trimmed.StartsWith("```json"))
                    {
                        skipBlock = true;
                        lines.RemoveAt(i);
                        continue;
                    }
                    if (skipBlock)
                    {
                        if (trimmed.StartsWith("```"))
                        {
                            skipBlock = false;
                            lines.RemoveAt(i);
                            continue;
                        }
                        lines.RemoveAt(i);
                    }
                    else if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        lines.RemoveAt(i);
                    }
                    else
                    {
                        break; // Dừng lại khi gặp dòng có nội dung thực sự
                    }
                }
                generatedText = string.Join('\n', lines);


                string addToCartMessage = "";
                // 3. Nếu user đang xác nhận mua hàng
                int? pendingProductId = session.GetInt32("pendingAddToCart");
                if (pendingProductId.HasValue)
                {
                    if (IsUserConfirmYes(request.UserInput))
                    {
                        // Thực hiện thêm vào giỏ
                        var product = _context.ProductList.FirstOrDefault(p => p.ProductID == pendingProductId.Value);
                        if (product != null)
                        {
                            var currentUser = _httpContextAccessor.HttpContext.User;
                            if (currentUser.Identity.IsAuthenticated)
                            {
                                string jsonCart = session.GetString(CartController.CARTKEY);
                                var cart = jsonCart != null ? JsonSerializer.Deserialize<List<CartItem>>(jsonCart) : new List<CartItem>();
                                var cartItem = cart.FirstOrDefault(c => c.ProductID == product.ProductID);
                                if (cartItem != null)
                                {
                                    cartItem.Quantity += 1;
                                }
                                else
                                {
                                    cart.Add(new CartItem { ProductID = product.ProductID, Quantity = 1 });
                                }
                                session.SetString(CartController.CARTKEY, JsonSerializer.Serialize(cart));
                                addToCartMessage = $"\n\n✅ Đã thêm **{product.ProductName}** vào giỏ hàng của bạn!";
                            }
                            else
                            {
                                addToCartMessage = "\n\n⚠️ Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
                            }
                        }
                        else
                        {
                            addToCartMessage = $"\n\n⚠️ Không tìm thấy sản phẩm phù hợp để thêm vào giỏ hàng.";
                        }

                        session.Remove("pendingAddToCart"); // Xóa sau khi xử lý
                    }
                    else if (IsUserConfirmNo(request.UserInput)) // THÊM DÒNG NÀY
                    {
                        // Người dùng từ chối => không thêm gì cả
                        session.Remove("pendingAddToCart");
                        addToCartMessage = "\n\n👍 Không thêm sản phẩm vào giỏ. Bạn cần hỗ trợ gì khác?";
                    }
                    // Nếu không rõ "có/không" thì giữ nguyên trạng thái chờ
                }
                // 4. Nếu phát hiện ý định mua hàng, hỏi xác nhận và lưu trạng thái
                else if (actionObj != null && actionObj.action == "add_to_cart" && !string.IsNullOrEmpty(actionObj.product_name))
                {
                    var product = _context.ProductList.FirstOrDefault(p => p.ProductName.ToLower().Contains(actionObj.product_name.ToLower()));
                    if (product != null)
                    {
                        session.SetInt32("pendingAddToCart", product.ProductID);
                        addToCartMessage = $"\n\nBạn có muốn đặt hàng sản phẩm **{product.ProductName}** không?";
                    }
                    else
                    {
                        addToCartMessage = $"\n\n⚠️ Không tìm thấy sản phẩm phù hợp để đặt hàng.";
                    }
                }

                string htmlResponse = Markdig.Markdown.ToHtml(generatedText + addToCartMessage);

                return Ok(new { success = true, html = htmlResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("widget")]
        public IActionResult GetChatWidget()
        {
            return PartialView("_ChatWidget");
        }

        // Thêm hàm kiểm tra xác nhận
        private bool IsUserConfirmYes(string input)
        {
            var yesWords = new[] { "có", "ok", "yes", "đồng ý", "mua", "đặt", "chắc chắn", "đúng rồi" };
            return yesWords.Any(w => input.Trim().ToLower().Contains(w));
        }
        bool IsUserConfirmNo(string userInput)
        {
                if (string.IsNullOrWhiteSpace(userInput))
                    return false;

                // Chuẩn hóa input: viết thường và trim khoảng trắng
                string normalizedInput = userInput.Trim().ToLowerInvariant();

                // Danh sách từ khóa biểu thị từ chối
                string[] noKeywords = new[]
                {
            "không",
            "không phải",
            "ko",
            "k",
            "hủy",
            "từ chối",
            "đừng",
            "không nhé",
            "không đâu",
            "không muốn",
            "chưa",
            "thôi"
        };

                // Kiểm tra xem input có chứa từ khóa nào không
                foreach (var keyword in noKeywords)
                {
                    // Dùng Contains để linh hoạt, bạn có thể dùng Equals hoặc Regex để chính xác hơn
                    if (normalizedInput.Contains(keyword))
                        return true;
                }

                return false;
            }
    }

    // Model phản hồi Gemini mới (chuẩn)
    public class GeminiGenerativeResponse
    {
        public List<GeminiCandidate> Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; }
    }

    public class GeminiPart
    {
        public string Text { get; set; }
            }

    public class ChatRequest
    {
        public string UserInput { get; set; }
        }

    // Thêm class GeminiAction để parse JSON
    public class GeminiAction
    {
        public string action { get; set; }
        public string product_name { get; set; }
    }
}