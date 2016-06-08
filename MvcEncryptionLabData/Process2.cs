using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MvcEncryptionLabData;

namespace MvcEncryptionLabData2
{
    public class ProcessStatus
    {
        public Guid ProcessId { get; set; }
        public string UserName { get; set; }
        public int PercentComplete { get; set; }
        public string Status { get; set; }
    }

    public class Process
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private int _percentComplete = 0;
        
        public Process()
        {
            this.ProcessId = Guid.NewGuid();
            this.SubProcesses = new List<Process>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProcessId { get; protected set; }
        public string UserName { get; set; }
        public Process Parent { get; set; }

        public List<Process> SubProcesses { get; set; }

        public DateTime? StartTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public virtual int PercentComplete
        {
            get
            {
                if (this.SubProcesses.Count == 0)
                {
                    return this._percentComplete;
                }
                else
                {
                    int valuePerPhase = (int)((float)100 / this.SubProcesses.Count);
                    int value = 0;

                    foreach (Process process in this.SubProcesses)
                    {
                        value += (valuePerPhase * process.PercentComplete);
                    }
                    return (int)((float)value / 100);
                }
            }

            set
            {
                if (this.SubProcesses.Count == 0)
                {
                    if (value < 0)
                    {
                        this._percentComplete = 0;
                    }
                    else if (value > 100)
                    {
                        this._percentComplete = 100;
                    }
                    else
                    {
                        this._percentComplete = value;
                    }
                }
            }
        }

        public string LogDescription
        {
            get
            {
                return String.Format(
                    "Process '{0}' [{1}]",
                    this.Name,
                    this.Description
                );
            }
        }

        public bool NotStarted
        {
            get
            {
                return !this.StartTime.HasValue;
            }
        }
        public bool Started
        {
            get
            {
                return this.StartTime.HasValue && !this.Duration.HasValue;
            }
        }
        public bool Finished
        {
            get
            {
                return this.Duration.HasValue;
            }
        }

        public LogItem Start()
        {
            this.PercentComplete = 0;
            this.StartTime = DateTime.Now;
            this._stopwatch.Start();
            return LogItem("starting");
        }

        public LogItem Finish()
        {
            this.PercentComplete = 100;
            this._stopwatch.Stop();
            this.Duration = this._stopwatch.Elapsed;
            this._stopwatch.Reset();
            return LogItem(
                String.Format(
                    "finished in {0} seconds",
                    this.Duration.Value.TotalSeconds.ToString("#,##0")
                )
            );
        }

        public LogItem LogItem(string text)
        {
            return new LogItem
            {
                Target = "",
                CreateDateTime = DateTime.Now,
                Text = String.Format(
                    "{0} - {1}.",
                    this.LogDescription,
                    text
                ),
                Type = Logger.LogItemType.Info,
                ProcessId = this.ProcessId,
                ProcessPercentComplete = this.PercentComplete
            };
        }

        public Process AddSubProcess(string name, string description)
        {
            Process process = new Process()
            {
                Name = name,
                Description = description,
                ProcessId = this.ProcessId,
                Parent = this
            };
            this.SubProcesses.Add(process);
            return process;
        }
    }
}
