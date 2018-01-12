namespace Com.Gmlan.Core.Scheduler
{
    public interface ISchedulerService<in T>
    {
        void Start();
        void Stop();
        void Run(T obj);
    }

}
