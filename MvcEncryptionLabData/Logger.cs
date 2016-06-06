using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class Logger
    {
        public enum LogItemType
        {
            Info,
            Warning,
            Error
        }

        public class LogItem
        {
            public LogItem()
            {
            }

            public LogItem(DateTime dateTime, string text, Logger.LogItemType type)
            {
                this.CreateDateTime = dateTime;
                this.Text = text;
                this.Type = type;
            }

            public int LogItemId { get; set; }

            public string Target { get; set; }

            public DateTime CreateDateTime { get; set; }

            public string Text { get; set; }

            public Logger.LogItemType Type { get; set; }

            public Guid ProcessId { get; set; }

            public int ProcessPercentComplete { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private List<LogItem> _log = new List<LogItem>();
        private Dictionary<LogItemType, int> _logItemCounts = new Dictionary<LogItemType, int>();

        public Logger()
        {
            this.Clear();
        }

        public List<LogItem> LogItems
        {
            get
            {
                return this._log;
            }
        }

        public void Clear()
        {
            this._log.Clear();
            this._logItemCounts.Clear();
            this._logItemCounts.Add(LogItemType.Info, 0);
            this._logItemCounts.Add(LogItemType.Warning, 0);
            this._logItemCounts.Add(LogItemType.Error, 0);
        }

        public int GetLogItemCount(LogItemType type)
        {
            return this._logItemCounts[type];
        }

        public void AddLogItem(
            LogItemType type,
            string text,
            int lineNumber = -1)
        {
            if (lineNumber > -1)
            {
                this.AddLogItem(
                    item: new LogItem
                    {
                        Type = type,
                        Text = String.Format(
                            "Line {0} - {1}.",
                            lineNumber,
                            text
                        ),
                        CreateDateTime = DateTime.Now
                    }
                );
            }
            else
            {
                this.AddLogItem(
                    item: new LogItem
                    {
                        Type = type,
                        Text = text,
                        CreateDateTime = DateTime.Now
                    }
                );
            }
        }

        public void AddLogItem(
            LogItemType type,
            string text,
            string target
        )
        {
            this.AddLogItem(
                item: new LogItem
                {
                    Type = type,
                    Text = text,
                    Target = target,
                    CreateDateTime = DateTime.Now
                }
            );
        }

        public void AddLogItem(LogItem item)
        {
            this._logItemCounts[item.Type] = this._logItemCounts[item.Type] + 1;
            this._log.Add(item);
        }

        public StringBuilder GetLog()
        {
            StringBuilder sb = new StringBuilder();
            this._log.ForEach(item => sb.AppendLine(item.ToString()));
            return sb;
        }

        public override string ToString()
        {
            return this.GetLog().ToString();
        }
    }
}
