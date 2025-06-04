using System.ComponentModel.DataAnnotations.Schema;

namespace BachHoaXanh.Models
{
    [Table("FaceData")]

    public class FaceData
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public byte[] FaceEmbedding { get; set; } // Lưu đặc trưng khuôn mặt dưới dạng byte array
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
    [Table("FaceAuthHistory")]

    public class FaceAuthHistory
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public DateTime AttemptTime { get; set; }
        public string Result { get; set; }
        public string? FailedImagePath { get; set; }
        public User User { get; set; }

    }
}
