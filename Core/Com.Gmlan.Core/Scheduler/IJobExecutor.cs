namespace Com.Gmlan.Core.Scheduler
{
    public interface IJobExecutor
    {
        void Execute(IJobHook hook, int jobId);
    }
}
