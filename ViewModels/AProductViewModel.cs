using BachHoaXanh.Models;
using static System.Net.Mime.MediaTypeNames;

namespace BachHoaXanh.ViewModels
{
    public class AProductViewModel
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CatID { get; set; }
        public int SubID { get; set; }
        public byte Active { get; set; }
        public string? Description { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }
}
