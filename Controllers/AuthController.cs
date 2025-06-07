using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BachHoaXanh.Data;
using Newtonsoft.Json;
using System.Text;
using BachHoaXanh.Models;
using System.Diagnostics;

namespace BachHoaXanh.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterFace(int userId, IFormFile image)
        {
            var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserID == userId && u.Role == "Admin");
            if (user == null) return Unauthorized("Không phải Admin");
            Console.WriteLine("➡️ Bắt đầu xử lý RegisterFace cho userId: " + userId);

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "face_images");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            string imagePath = Path.Combine(uploadPath, $"admin_{userId}_{DateTime.Now.Ticks}.jpg");

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            Console.WriteLine("Save RegisterFace cho userId: " + userId);

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"face_recognition_script.py encode_image \"{imagePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("❌ Lỗi từ Python: " + error);
                        return Json(new { success = false, message = error });
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(output);
                    bool success = result.success;

                    if (!success)
                    {
                        string msg = result.message.ToString();
                        Console.WriteLine("❌ encode_image thất bại: " + msg);
                        return Json(new { success = false, message = msg });
                    }

                    Console.WriteLine("💾 Đang serialize embedding...");
                    string embedding = JsonConvert.SerializeObject(result.embedding);

                    Console.WriteLine("📝 Lưu vào DB...");
                    _context.FaceData.Add(new FaceData
                    {
                        UserID = userId,
                        FaceEmbedding = Encoding.UTF8.GetBytes(embedding),
                        CreatedAt = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ Lưu DB thành công!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi xử lý: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi xử lý: {ex.Message}" });
            }

            return Json(new { success = true, message = "Đăng ký khuôn mặt thành công!" });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyFace(int userId, IFormFile image)
        {
            var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserID == userId && u.Role == "Admin");
            if (user == null) return Unauthorized("Không phải Admin");

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "face_images");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            string imagePath1 =$"{userId}_{DateTime.Now.Ticks}.jpg";
            string imagePath = Path.Combine(uploadPath, imagePath1);
            
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var faceData = await _context.FaceData.FirstOrDefaultAsync(f => f.UserID == userId);
            if (faceData == null)
            {
                await LogAttempt(userId, "Failed", imagePath1);
                return Json(new { success = false, message = "Chưa đăng ký khuôn mặt!" });
            }

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Users\ADMIN\AppData\Local\Programs\Python\Python39\python.exe",
                    Arguments = $"face_recognition_script.py authenticate_image \"{imagePath}\" \"{Encoding.UTF8.GetString(faceData.FaceEmbedding)}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        await LogAttempt(userId, "Failed", imagePath1);
                        return Json(new { success = false, message = error });
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(output);
                    bool success = result.success;

                    if (!success)
                    {
                        await LogAttempt(userId, "Failed", imagePath1);
                        return Json(new { success = false, message = result.message.ToString() });
                    }

                    bool match = result.match;
                    if (!match)
                    {
                        await LogAttempt(userId, "Failed", imagePath1);
                        return Json(new { success = false, message = "Khuôn mặt không khớp!" });
                    }

                    await LogAttempt(userId, "Success", imagePath1);
                    return Json(new { success = true, message = "Xác thực thành công!", redirectUrl = "/Admin/Index" });
                }
            }
            catch (Exception ex)
            {
                await LogAttempt(userId, "Failed", imagePath1);
                return Json(new { success = false, message = $"Lỗi xử lý: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult FaceAuth(int userId)
        {
            var user = _context.UserList.FirstOrDefault(u => u.UserID == userId && u.Role == "Admin");
            if (user == null) return Unauthorized("Không phải Admin");
            return View(userId);
        }
        [HttpGet]
        public IActionResult RegisterFace(int userId)
        {
            var user = _context.UserList.FirstOrDefault(u => u.UserID == userId && u.Role == "Admin");
            if (user == null) return Unauthorized("Không phải Admin");
            return View(userId);
        }
        private async Task LogAttempt(int userId, string status, string failedImagePath)
        {
            _context.FaceAuthHistory.Add(new FaceAuthHistory
            {
                UserID = userId,
                AttemptTime = DateTime.Now,
                Result = status,
                FailedImagePath = failedImagePath
            });
            await _context.SaveChangesAsync();
        }
    }
}