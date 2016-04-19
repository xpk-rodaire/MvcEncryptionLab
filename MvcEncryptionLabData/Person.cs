using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MvcEncryptionLabData
{
    public class Person
    {
        public int PersonId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [NotMapped]
        public string LastName { get; set; }

        [StringLength(255)]
        public string LastNameEncrypted { get; set; }

        [NotMapped]
        public string SSN { get; set; }

        [StringLength(64)]
        public string SSNIV { get; set; }

        [StringLength(255)]
        public string SSNEncrypted { get; set; }

        [StringLength(64)]
        public string SSNSalt { get; set; }

        [StringLength(64)]
        public string SSNHash { get; set; }

        public Address Address { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(
                "SSN = {0}; LN = {1}; FN = {2}; Zip = {3}",
                this.SSN,
                this.LastName,
                this.FirstName,
                this.Address.Zip
            );

            return sb.ToString();
        }
    }
}
