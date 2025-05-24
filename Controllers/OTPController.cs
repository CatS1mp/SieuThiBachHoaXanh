using BachHoaXanh.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BachHoaXanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OTPController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> OtpStore = new();
        private readonly IEmailService _emailService;
        private readonly OtpSettings _otpSettings;

        public OTPController(IEmailService emailService, IOptions<OtpSettings> otpSettings)
        {
            _emailService = emailService;
            _otpSettings = otpSettings.Value;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required");
            }

            // Generate 6-digit OTP
            string otp = new Random().Next(100000, 999999).ToString();
            DateTime expiry = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);

            // Store OTP
            OtpStore[request.Email] = (otp, expiry);

            // Send OTP via email
            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otp);
                return Ok(new { Message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to send OTP", Error = ex.Message });
            }
        }

        [HttpPost("verify")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest("Email and OTP are required");
            }

            if (!OtpStore.TryGetValue(request.Email, out var storedOtp))
            {
                return BadRequest("No OTP found for this email");
            }

            if (storedOtp.Expiry < DateTime.UtcNow)
            {
                OtpStore.TryRemove(request.Email, out _);
                return BadRequest("OTP has expired");
            }

            if (storedOtp.Otp != request.Otp)
            {
                return BadRequest("Invalid OTP");
            }

            // OTP is valid, remove it
            OtpStore.TryRemove(request.Email, out _);
            return Ok(new { Message = "OTP verified successfully" });
        }
    }
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string email, string otp);
    }

    public class EmailService : IEmailService
    {
        private readonly OtpSettings _otpSettings;

        public EmailService(IOptions<OtpSettings> otpSettings)
        {
            _otpSettings = otpSettings.Value;
        }

        public async Task SendOtpEmailAsync(string email, string otp)
        {
            using var client = new SmtpClient(_otpSettings.Smtp.Host, _otpSettings.Smtp.Port)
            {
                EnableSsl = _otpSettings.Smtp.EnableSSL,
                Credentials = new NetworkCredential(_otpSettings.Smtp.Username, _otpSettings.Smtp.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_otpSettings.Smtp.Username, "OTP Service"),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is {otp}. It is valid for {_otpSettings.ExpiryMinutes} minutes.",
                IsBodyHtml = false
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
