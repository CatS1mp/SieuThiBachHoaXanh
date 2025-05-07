using BachHoaXanh.Models;

namespace BachHoaXanh.ViewModels
{
    public class AdminViewModel
    {
        public List<Order>? Orders { get; set; }
        public List<Product>? Products { get; set; }
        public List<User>? Users { get; set; }
        public List<Category>? Categories { get; set; }
        public List<SubCategory>? SubCategories { get; set; }
        public List<PaymentMethod>? PaymentMethods { get; set; }
    }
}
