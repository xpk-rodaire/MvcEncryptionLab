using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcEncryptionLabData
{
    public class Address
    {
        public int AddressId { get; set; }

        [NotMapped]
        public string AddressLine1 { get; set; }

        [StringLength(64)]
        public string AddressLine1IV { get; set; }

        [StringLength(255)]
        public string AddressLine1Encrypted { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
