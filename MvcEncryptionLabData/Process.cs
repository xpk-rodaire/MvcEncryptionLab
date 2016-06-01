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
        private Stopwatch _stopWatch;
        private DateTime _startTime;
        private TimeSpan _duration;

        public AbstractProcess()
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public void Start()
        {
            this._startTime = DateTime.Now;
            this._stopWatch.Start();
        }

        public DateTime StartTime
        { 
            get
            {
                return this._startTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return this._startTime.Add(this._duration);
            }
        }

        public void End()
        {
            this._duration = this._stopWatch.Elapsed;
        }

        public int PercentProgress { get; set; }

    }

    public class Process : AbstractProcess
    {
        public Process()
        {
            this.Phases = new List<ProcessPhase>();
        }

        public List<ProcessPhase> Phases { get; private set; }
    }

    public class ProcessPhase : AbstractProcess
    {
        private int _totalRecords;
        private int _recordsProcessed;

        public int TotalRecords { get; set; }

        public int RecordsProcessed { get; set; }

        public int PercentProgress 
        { 
            get
            {
                if (this.RecordsProcessed > 0 && this.TotalRecords > 0)
                {
                    return (int)((float)this.RecordsProcessed / this.TotalRecords);
                }
                return 0;
            }
            
            set
            {

            }
        }
    }
}
