using BachHoaXanh.Models;

namespace BachHoaXanh.ViewModels
{
    public class ProductViewModel
    {

        public List<FavoriteProduct> FavoriteProducts { get; set; }
        public List<Promotion> Promotions { get; set; }
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
        public int TotalProducts { get; set; }
        public int? SubCategoryId { get; set; }

    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public bool isFav { get; set; }
    }
}