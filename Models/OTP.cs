namespace BachHoaXanh.Models
{
    public class OtpRequest
    {
        public string Email { get; set; }
    }

    public class OtpVerificationRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
    public class OtpSettings
    {
        public int ExpiryMinutes { get; set; }
        public SmtpSettings Smtp { get; set; }
    }

    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
