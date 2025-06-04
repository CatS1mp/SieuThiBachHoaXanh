namespace BachHoaXanh.Models
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public int ProductID { get; set; }
        public string? Note { get; set; } = "";
        public int PaymentMethodID { get; set; } = 1;
        // Not serialized: loaded from DB
        [System.Text.Json.Serialization.JsonIgnore]
        public Product Product { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<ProductImage> ProductImages { get; set; }
    }
}