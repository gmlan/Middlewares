using Com.Gmlan.Core.Model.Scheduler;
using Quartz;
using System;

namespace Com.Gmlan.Scheduler.JobInjector
{
    public static class JobExtension
    {
        public static bool NeedRun(this Job job)
        {
            var cronExpression = new CronExpression(job.ScheduledTime);
            DateTimeOffset utcOffset = DateTime.SpecifyKind(job.LastRunningTime, DateTimeKind.Utc);
            var afterTime = cronExpression.GetTimeAfter(utcOffset);

            if (!afterTime.HasValue)
                throw new Exception($"Can not parse the ScheduledTime of job: {job}");

            return DateTime.UtcNow > afterTime.Value.ToUniversalTime();
        }
    }
}
