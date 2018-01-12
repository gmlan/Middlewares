using System;

namespace Com.Gmlan.Scheduler.Model
{
    public class ProcessCache<T> : CacheEntity<T>
    {

        public string IpAddress { get; set; }

        public string MacAddress { get; set; }

        public int ProcessId { get; set; }

        public int ThreadId { get; set; }
        public string MachineKey { get; set; }

        public DateTime CompletedTimeUtc { get; set; }
    }
}
