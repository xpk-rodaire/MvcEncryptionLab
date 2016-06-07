using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MvcEncryptionLabData
{
    public class ProcessStatus
    {
        public Guid ProcessId { get; set; }
        public string UserName { get; set; }
        public int PercentComplete { get; set; }
        public string Status { get; set; }
    }

    public abstract class AbstractProcess
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private int _percentComplete = 0;

        public AbstractProcess()
        {
            this.ProcessId = Guid.NewGuid();
        }

        public string Name { get; set;  }
        public string Description { get; set; }
        public Guid ProcessId { get; protected set; }
        public string UserName { get; set; }

        public DateTime? StartTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public virtual int PercentComplete
        {
            get
            {
                return this._percentComplete;
            }

            set
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

        protected abstract string LogDescription { get; }

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
    }

    public class Process : AbstractProcess
    {
        public Process()
            : base()
        {
            this.Phases = new List<ProcessPhase>();
        }

        public override int PercentComplete
        {
            get
            {
                int valuePerPhase = (int)((float)100 / this.Phases.Count);
                int value = 0;

                foreach (ProcessPhase phase in this.Phases)
                {
                    value += (valuePerPhase * phase.PercentComplete);
                }
                return (int)((float)value / 100);
            }
        }

        protected override string LogDescription
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

        public ProcessPhase AddProcessPhase(string name, string description, int number)
        {
            ProcessPhase newPhase = new ProcessPhase(this, this.ProcessId)
            {
                Name = name,
                Description = description,
                PhaseNumber = number
            };
            this.Phases.Add(newPhase);
            return newPhase;
        }

        public List<ProcessPhase> Phases { get; set; }
    }

    public class ProcessPhase : AbstractProcess
    {
        private Process _parent;

        public ProcessPhase(Process parent, Guid processId)
            : base()
        {
            this._parent = parent;
            this.ProcessId = processId;
        }

        protected override string LogDescription
        {
            get
            {
                return String.Format(
                    "Process phase '{0}' [{1} of {2}]",
                    this.Name,
                    this.PhaseNumber,
                    this._parent.Phases.Count()
                );
            }
        }

        public int PhaseNumber { get; set; }
    }
}
