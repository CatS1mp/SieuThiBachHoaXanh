using BachHoaXanh.Data;
using BachHoaXanh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.IO;
using System.Threading.Tasks;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult RegisterFace(int userId)
    {
        var user = _context.UserList.FirstOrDefault(u => u.UserID == userId && u.Role == "Admin");
        if (user == null) return Unauthorized("Không phải Admin");
        return View(userId);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterFace(int userId, IFormFile image)
    {
        var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserID == userId && u.Role == "Admin");
        if (user == null) return Unauthorized("Không phải Admin");

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "face_images");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
        string imagePath = Path.Combine(uploadPath, $"admin_{userId}_{DateTime.Now.Ticks}.jpg");

        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        _context.FaceData.Add(new FaceData
        {
            UserID = userId,
            FaceImagePath = imagePath,
            CreatedAt = DateTime.Now
        });
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đăng ký khuôn mặt thành công!" });
    }

    [HttpGet]
    public IActionResult FaceAuth(int userId)
    {
        var user = _context.UserList.FirstOrDefault(u => u.UserID == userId && u.Role == "Admin");
        if (user == null) return Unauthorized("Không phải Admin");
        return View(userId);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyFace(int userId, IFormFile image)
    {
        var user = await _context.UserList.FirstOrDefaultAsync(u => u.UserID == userId && u.Role == "Admin");
        if (user == null) return Unauthorized("Không phải Admin");

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "face_images");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
        string imagePath = $"{userId}_{DateTime.Now.Ticks}.jpg";
        string imagePath2 = Path.Combine(uploadPath, imagePath);

        using (var stream = new FileStream(imagePath2, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        var cascade = new CascadeClassifier("haarcascades/haarcascade_frontalface_default.xml");
        using var img = Cv2.ImRead(imagePath2);
        var faces = cascade.DetectMultiScale(img, 1.1, 3);

        if (faces.Length == 0)
        {
            await LogAttempt(userId, "Failed", imagePath);
            return Json(new { success = false, message = "Không phát hiện khuôn mặt, thử lại!" });
        }

        var faceData = await _context.FaceData.FirstOrDefaultAsync(f => f.UserID == userId);
        if (faceData == null)
        {
            await LogAttempt(userId, "Failed", imagePath);
            return Json(new { success = false, message = "Chưa đăng ký khuôn mặt!" });
        }

        // Giả lập: Nếu phát hiện khuôn mặt và đã có dữ liệu đăng ký, coi như thành công
        await LogAttempt(userId, "Success", imagePath);
        return Json(new { success = true, message = "Xác thực thành công!", redirectUrl = "/Admin/Index" });
    }

    private async Task LogAttempt(int userId, string result, string? imagePath)
    {
        _context.FaceAuthHistory.Add(new FaceAuthHistory
        {
            UserID = userId,
            AttemptTime = DateTime.Now,
            Result = result,
            FailedImagePath =imagePath,
        });
        await _context.SaveChangesAsync();
    }
}