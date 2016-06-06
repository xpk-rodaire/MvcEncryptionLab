using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class LogItem
    {
        public LogItem()
        {
        }

        public LogItem(string user, string target, DateTime dateTime, string text)
        {
            this.UserName = user;
            this.Target = target;
            this.CreateDateTime = dateTime;
            this.Text = text;
        }

        public int LogItemId { get; set; }

        [MaxLength(25)]
        public string UserName { get; set; }

        [MaxLength(255)]
        public string Target { get; set; }

        public DateTime CreateDateTime { get; set; }

        [MaxLength(5000)]
        public string Text { get; set; }

        public Logger.LogItemType Type { get; set; }

        public Guid ProcessId { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage complete must be between 0 and 100, inclusive.")]
        public int ProcessPercentComplete { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        //[Required]
        //public string FakeFIeld { get; set; }
    }
}
