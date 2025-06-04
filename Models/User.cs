using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string? Password { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự.")]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Vai trò không quá 20 ký tự.")]
        public string? Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal Points { get; set; } = 0;

        [StringLength(50)]
        public string? Rank { get; set; } = "Chưa xếp hạng";

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}