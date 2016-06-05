using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MvcEncryptionLabData
{
    public abstract class AbstractProcess
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private int _percentComplete = 0;
        private Logger _logger;

        public AbstractProcess(Logger logger)
        {
            this._logger = logger;
            this.ProcessId = Guid.NewGuid();
        }

        public string Name { get; set;  }
        public string Description { get; set; }
        public Guid ProcessId { get; protected set; }
        public string UserName { get; set; }

        public DateTime? StartTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public int PercentComplete
        {
            get
            {
                // Based on progress of phases?
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

        public Logger Logger
        {
            get
            {
                return this._logger;
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

        public void Start()
        {
            this.LogItem(
                target: "",
                text: String.Format(
                    "Starting {0}",
                    this.LogDescription,
                    this.Description
                )
            );
            this.StartTime = DateTime.Now;
            this._stopwatch.Start();
        }

        public void Finish()
        {
            this._stopwatch.Stop();
            this.Duration = this._stopwatch.Elapsed;
            this.LogItem(
                target: "",
                text: String.Format(
                    "Finished {0} in {1} seconds",
                    this.LogDescription,
                    this.Duration.Value.TotalSeconds.ToString("#,##0")
                )
            );
            this._stopwatch.Reset();
        }

        protected void LogItem(string target, string text)
        {
            this.Logger.AddLogItem(
                new Logger.LogItem
                {
                    Target = target,
                    CreateDateTime = DateTime.Now,
                    Text = text,
                    Type = Logger.LogItemType.Info,
                    ProcessId = this.ProcessId,
                    ProcessPercentComplete = this.PercentComplete
                }
            );
        }
    }

    public class Process : AbstractProcess
    {
        public Process(Logger logger)
            : base(logger)
        {
            this.Phases = new List<ProcessPhase>();
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

        public List<ProcessPhase> Phases { get; set; }
    }

    public class ProcessPhase : AbstractProcess
    {
        private Process _parent;

        public ProcessPhase(Logger logger, Process parent)
            : base(logger)
        {
            this._parent = parent;
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
