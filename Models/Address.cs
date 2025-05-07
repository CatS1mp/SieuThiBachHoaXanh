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
}
