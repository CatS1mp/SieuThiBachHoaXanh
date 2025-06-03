using BachHoaXanh.Models;

namespace BachHoaXanh.ViewModels
{
    public class UpdateProfileView
    {
        public int UserId { get; set; } // Thêm thuộc tính UserId
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal Points { get; set; } // Thêm thuộc tính Points
        public string Rank { get; set; } // Thêm thuộc tính Rank
        public List<Address> Addresses { get; set; }
    }
}