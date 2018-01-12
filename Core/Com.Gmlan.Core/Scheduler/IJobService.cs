using Com.Gmlan.Core.Model.Scheduler;
using System.Linq;

namespace Com.Gmlan.Core.Scheduler
{
    public interface IJobService
    {
        IQueryable<Job> GetAll(bool noTracking = false);

        Job GetById(int id);

        void Update(Job job);

        void Delete(params Job[] job);

        void Insert(params Job[] entities);

    }
}
