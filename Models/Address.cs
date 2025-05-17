using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BachHoaXanh.Models
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(255)]
        public string Province { get; set; }

        [StringLength(255)]
        public string District { get; set; }

        [StringLength(255)]
        public string Ward { get; set; }

        [StringLength(255)]
        public string Street { get; set; }

        public bool IsDefault { get; set; } = false;

        public User User { get; set; }
    }
    public class AddressViewModel
    {
        public int AddressID { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Street { get; set; }
        public bool IsDefault { get; set; }
    }
    public class UpdateProfileView
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
