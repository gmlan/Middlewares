using Autofac;
using Com.Gmlan.Core.Cache;
using Com.Gmlan.Core.Extension;
using Com.Gmlan.Core.Helper;
using Com.Gmlan.Core.MessageQueue;
using Com.Gmlan.Core.Model.Scheduler;
using Com.Gmlan.Core.Scheduler;
using Com.Gmlan.Scheduler.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Com.Gmlan.Scheduler
{

    public class JobExecutor : IJobExecutor, IMessageQueueConsumer
    {
        const string CACHED_JOB_KEY = "Com.Gmlan.Core.Models.Scheduler.Job_{0}";


        private readonly ILog _log;
        private readonly IJobService _jobService;
        private readonly ICacheManager _cacheManager;
        private readonly ILifetimeScope _lifetimeScope;

        private readonly IList<Type> _jobHooks;

        public JobExecutor(ILog log, IJobService jobService, ICacheManager cacheManager, ILifetimeScope lifetimeScope)
        {
            _log = log;
            _jobService = jobService;
            _cacheManager = cacheManager;
            _lifetimeScope = lifetimeScope;

            _jobHooks = AssemblyHelper.FindAllConcreteTypesFromPlugin(typeof(IJobHook));
        }

        private void UpdateJobStatus(JobStatus status, Job job)
        {
            _log.Info($"Statu changed : {job.Status} -> {status}, JobId: {job.Id}");

            job.Status = status;

            //LastRunningTime only keep the time point when the job start, otherwise those data inserted between
            //job started and job stopped will be skipped if we set LastRunningTime when status is Completed or Stopped
            if (job.Status == JobStatus.Running)
                job.CheckPointTime = DateTime.UtcNow;

            if (job.Status == JobStatus.Stopped || job.Status == JobStatus.Completed)
                job.LastRunningTime = job.CheckPointTime;//Update LastRunningTime

            _jobService.Update(job);
        }


        public void Execute(IJobHook hook, int jobId)
        {
            Job job = null;
            try
            {
                //attach a job object to job service
                job = _jobService.GetById(jobId);

                UpdateJobStatus(JobStatus.Running, job);
                hook.StartJob(job);
                UpdateJobStatus(JobStatus.Completed, job);
            }
            catch (Exception ex)
            {
                if (job != null)
                {
                    _log.Error($"{ex.Message} : {job.FormatToString()}", ex);
                    UpdateJobStatus(JobStatus.Stopped, job);
                }
            }
        }

        public void Consume(QueueMessageModel model)
        {
            var job = model.Data as Job;
            if (job == null)
                return;

            var key = string.Format(CACHED_JOB_KEY, job.Id);
            var status = _cacheManager.Get<ProcessCache<JobStatus>>(key);
            if (status != null)
            {
                //Injection time is early than Last completed time
                //Injection time is set in JobInjector
                if (job.CheckPointTime < status.CompletedTimeUtc)
                    return;

                if (status.Value == JobStatus.Running)
                {
                    //Check the service is still running by heartbeat, otherwise maybe crashed
                    var heatBeat = _cacheManager.Get<CacheEntity<int>>(status.MachineKey);
                    if (heatBeat != null)
                        return;
                }
            }

            //Set Cache to stop other thread, Default to cache for 1 day
            var cacheEntity = new ProcessCache<JobStatus>()
            {
                IpAddress = HostInfo.IpAddress,
                MacAddress = HostInfo.MacAddress,
                MachineKey = HostInfo.MachineKey,
                ProcessId = Process.GetCurrentProcess().Id,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                CreateTimeUtc = DateTime.UtcNow,
                Value = JobStatus.Running
            };

            _cacheManager.Set(key, cacheEntity, 60 * 24);

            try
            {
                var type = _jobHooks.FirstOrDefault(m => m.FullName == job.FullName);
                if (type != null)
                {
                    var hook = _lifetimeScope.Resolve(type) as IJobHook;
                    Execute(hook, job.Id);
                }

                cacheEntity.Value = JobStatus.Completed;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                cacheEntity.Value = JobStatus.Stopped;
            }
            finally
            {
                cacheEntity.CompletedTimeUtc = DateTime.UtcNow;
                _cacheManager.Set(key, cacheEntity, 60 * 24); //Update
            }
        }
    }

}
