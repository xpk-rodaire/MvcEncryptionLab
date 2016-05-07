using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcEncryptionLabData
{
    public class EncryptedCheckPhrase
    {
        public int EncryptedCheckPhraseId { get; set; }

        [NotMapped()]
        public string CheckPhrase { get; set; }

        [StringLength(64)]
        public string CheckPhraseIV { get; set; }

        [StringLength(255)]
        public string CheckPhraseEncrypted { get; set; }
    }
}
