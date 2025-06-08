using BachHoaXanh.Models;

namespace BachHoaXanh.ViewModels
{
    public class FaceAuthienticationView
    {
        public List<FaceData> FaceData { get; set; }
        public List<FaceAuthHistory>  FaceAuthHistory { get; set; }
        public List<User> User{ get; set; }
    }
}
