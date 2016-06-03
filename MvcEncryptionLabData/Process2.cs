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

        public string Name { get; set;  }
        public string Description { get; set; }
        DateTime StartTime { get; set; }
        TimeSpan Duration { get; set; }
        int PercentComplete { get; set; }
        
        public void Start()
        {
            this.StartTime = DateTime.Now;
            this._stopwatch.Start();
        }

        public void Finish()
        {
            this._stopwatch.Stop();
            this.Duration = this._stopwatch.Elapsed;
            this._stopwatch.Reset();
        }
    }

    public class Process : AbstractProcess
    {
        public Process()
        {
            this.Phases = new List<ProcessPhase>();
        }

        public int ProcessId { get; set; }

        public virtual List<ProcessPhase> Phases { get; private set; }
    }

    public class ProcessPhase : AbstractProcess
    {
        public ProcessPhase()
        {
        }

        public int ProcessPhaseId { get; set; }

        public Process Process { get; set; }
    }
}
