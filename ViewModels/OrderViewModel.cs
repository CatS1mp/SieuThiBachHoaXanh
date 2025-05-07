namespace BachHoaXanh.ViewModels
{

    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public string UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public int TotalQuantity { get; set; }
        public List<OrderDetailItemViewModel> Products { get; set; }
    }

    public class OrderDetailItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public string Total { get; set; }
        public string Image { get; set; }
    }
}
