using System;

namespace Com.Gmlan.Scheduler.Model
{
    public class CacheEntity<T>
    {

        public T Value { get; set; }

        public DateTime CreateTimeUtc { get; set; }
    }
}
