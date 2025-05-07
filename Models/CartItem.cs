namespace BachHoaXanh.Models
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public int ProductID { get; set; }
        public List<ProductImage> ProductImages { get; set; }

        public string? Note { get; set; } = "";
        public int PaymentMethodID { get; set; } = 1;
    }
}
