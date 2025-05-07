using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.ViewModels
{
    public class LoginView
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
