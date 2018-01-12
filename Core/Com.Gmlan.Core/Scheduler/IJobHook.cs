using Com.Gmlan.Core.Model.Scheduler;
using System;

namespace Com.Gmlan.Core.Scheduler
{
    public interface IJobHook
    {
        void StartJob(Job job);

        Type GetHookParameter();
    }

}
