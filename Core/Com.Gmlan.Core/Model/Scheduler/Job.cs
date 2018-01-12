using Com.Gmlan.Core.Extension;
using Com.Gmlan.Core.Helper;
using Com.Gmlan.Core.Scheduler;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Com.Gmlan.Core.Model.Scheduler
{
    public enum JobStatus
    {
        NotStart,
        Running,
        Stopped,
        Completed
    }

    public class Job : BaseEntity
    {
        [Column(Order = 1)]
        public string Name { get; set; }

        [Column(Order = 2)]
        public string Description { get; set; }

        [Column(Order = 3)]
        public DateTime LastRunningTime { get; set; } = new DateTime(1900, 1, 1);

        //No need to save to db
        [NotMapped]
        public DateTime CheckPointTime { get; set; } = new DateTime(1900, 1, 1);

        [Column(Order = 4)]
        public JobStatus Status { get; set; }

        [Column(Order = 5)]
        public string FullName { get; set; }

        [Column(Order = 6)]
        public string ScheduledTime { get; set; }

        [Column(Order = 7)]
        public string Parameter
        {
            get { return HookParameter.ToJson(); }
            set
            {
                var hooks = AssemblyHelper.FindAllConcreteTypesFromPlugin(typeof(IJobHook));
                if (!hooks.Any())
                    hooks = AssemblyHelper.FindAllConcreteTypes(typeof(IJobHook));
                foreach (var hook in hooks)
                {
                    if (hook.FullName == FullName)
                    {
                        var hookInstance = Activator.CreateInstance(hook) as IJobHook;
                        if (hookInstance != null)
                            HookParameter = value.FromJson(hookInstance.GetHookParameter()) as HookParameter;
                        break;
                    }
                }
            }
        }

        [NotMapped]
        public HookParameter HookParameter { get; set; }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
