using BachHoaXanh.Data;
using Microsoft.AspNetCore.Mvc;
using Markdig;
using BachHoaXanh.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class ChatRequest
        {
            [Required]
            public string? userinput { get; set; }
        }

        public class AIResponse
        {
            [Required]
            public string? reply { get; set; }
        }

        [HttpPost("GeminiChat")]
        public async Task<JsonResult> GeminiChat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.userinput))
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Vui lòng nhập nội dung câu hỏi" 
                    });
                }

                var systemPrompt = "Bạn là trợ lý ảo của siêu thị Bách Hóa Xanh, " +
                    "nhiệm vụ của bạn là hỗ trợ khách hàng về các vấn đề sau:\n" +
                    "1. Tư vấn và cung cấp thông tin về sản phẩm\n" +
                    "2. Hướng dẫn cách tìm kiếm sản phẩm trên website\n" +
                    "3. Giải đáp thắc mắc về giá cả, khuyến mãi\n" +
                    "4. Hỗ trợ về quy trình đặt hàng và thanh toán\n" +
                    "5. Giải đáp các câu hỏi về chính sách bảo hành, đổi trả\n\n" +
                    "Khi trả lời, hãy:\n" +
                    "- Sử dụng ngôn ngữ thân thiện, lịch sự\n" +
                    "- Trả lời ngắn gọn, dễ hiểu\n" +
                    "- Sử dụng Markdown để định dạng văn bản cho dễ đọc\n" +
                    "- Với sản phẩm: **Tên sản phẩm** - *Giá: xxx đ*\n" +
                    "- Luôn chủ động hỏi thêm nếu cần thông tin để tư vấn tốt hơn";

                // Lấy thông tin sản phẩm từ database
                var products = await _context.ProductList
                    .Where(p => p.IsActive)
                    .Select(p => new { 
                        p.ProductName, 
                        p.Description, 
                        p.Price, 
                        p.StockQuantity,
                        p.IsActive
                    })
                    .ToListAsync();

                var categories = await _context.CategoryList
                    .Select(c => new { c.CategoryName })
                    .ToListAsync();

                var subcategories = await _context.SubCategoryList
                    .Select(sc => new { sc.SubCategoryName })
                    .ToListAsync();

                // Định dạng thông tin sản phẩm
                var productInfo = string.Join("\n", products.Select(p => 
                    $"- **{p.ProductName}**\n  - Mô tả: {p.Description}\n  - *Giá: {p.Price:N0}đ*\n  - Kho: {p.StockQuantity} sản phẩm"
                ));
                
                var categoryInfo = string.Join(", ", categories.Select(c => c.CategoryName));
                var subcategoryInfo = string.Join(", ", subcategories.Select(sc => sc.SubCategoryName));

                var fullContext = $"Danh mục sản phẩm: {categoryInfo}\n" +
                                $"Danh mục phụ: {subcategoryInfo}\n\n" +
                                $"Thông tin sản phẩm hiện có:\n{productInfo}";

                var fullPrompt = $"{systemPrompt}\n\nDữ liệu hệ thống:\n{fullContext}\n\nKhách hàng hỏi: {request.userinput}";

                using var httpClient = new HttpClient();
                var url = "http://103.82.36.41:5000/api/getChat";

                var data = new { userinput = fullPrompt };
                var response = await httpClient.PostAsJsonAsync(url, data);

                if (!response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Xin lỗi, hiện tại hệ thống đang gặp sự cố. Vui lòng thử lại sau." 
                    });
                }

                var result = await response.Content.ReadFromJsonAsync<AIResponse>();

                if (result == null || string.IsNullOrEmpty(result.reply))
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Xin lỗi, không thể xử lý câu hỏi của bạn. Vui lòng thử lại." 
                    });
                }

                // Chuyển đổi Markdown sang HTML
                var htmlResponse = Markdown.ToHtml(result.reply ?? string.Empty);

                return new JsonResult(new { 
                    success = true, 
                    html = htmlResponse 
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = "Đã xảy ra lỗi: " + ex.Message 
                });
            }
        }
    }
}
