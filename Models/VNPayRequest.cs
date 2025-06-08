namespace BachHoaXanh.Models
{
    public class VNPayRequest
    {
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public string OrderDescription { get; set; }
        public string CreatedDate { get; set; }
        public string ClientIp { get; set; }
    }
}
