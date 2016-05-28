using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcEncryptionLabData
{
    public class SecurityKey
    {
        public int SecurityKeyId { get; set; }

        [StringLength(64)]
        [Required]
        public string SecurityKeySalt { get; set; }

        [StringLength(64)]
        [Required]
        public string SecurityKeyHash { get; set; }
    }
}
