using Com.Gmlan.Core.Resolver;
using Com.Gmlan.Core.Scheduler;
using Com.Gmlan.Core.System;
using Com.Gmlan.Middlewares.MessageQueue.Client.Generic;
using Com.Gmlan.Middlewares.MessageQueue.Model.Generic;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Gmlan.Scheduler.JobInjector
{

    public class SchedulerService : ISchedulerService<CancellationToken>
    {

        #region fields

        private readonly ILog _log;
        private Task _schedulerTask;
        private AssemblyResolver _assemblyResolver;
        private readonly IJobService _jobService;


        readonly CancellationTokenSource _cancellationTokenSource;
        readonly CancellationToken _cancellationToken;

        private readonly RabbitMqSender<MessageBody> _rabbitMqSender;
        #endregion

        public SchedulerService(ISettingService settingService, IJobService jobService, ILog log)
        {
            _jobService = jobService;
            _log = log;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _rabbitMqSender = new RabbitMqSender<MessageBody>(
                settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_RABBITMQ_CONNECTIONSTRING),
                settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_RABBITMQ_DEFAULT_EXCHANGE),
                settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_RABBITMQ_DEFAULT_ROUTEKEY));
        }


        public void Start()
        {
            _assemblyResolver = new AssemblyResolver(_log);
            _assemblyResolver.Attach();

            _schedulerTask = Task.Run(() => Run(_cancellationToken), _cancellationToken);

            _log.Info("Service started");
        }


        public void Stop()
        {
            try
            {
                _assemblyResolver?.Detach();
                _cancellationTokenSource.Cancel();
                _schedulerTask.Wait(_cancellationToken);
            }
            catch (System.Exception ex)
            {
                _log.Error($"Exception thrown when try to stop the service : {ex.Message}", ex);
            }

            _log.Info("Service stopped");
        }


        #region Methods.


        public void Run(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    foreach (var job in _jobService.GetAll(true))
                    {
                        try
                        {
                            if (!job.NeedRun()) continue;

                            job.CheckPointTime = DateTime.UtcNow;//Injection time

                            //Send to Message Queue
                            //Com.Gmlan.Scheduler.TaskExecutor, Com.Gmlan.Scheduler will be responsible for all task execution
                            //So this project won't rely on any Hook assemblies
                            _rabbitMqSender.Send(
                                new MessageBody(
                                    "Com.Gmlan.Scheduler.JobExecutor, Com.Gmlan.Scheduler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                                    "Consume", job));
                        }
                        catch (System.Exception ex)
                        {
                            _log.Error(ex.Message, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);//For Database connection issue, log to file appender
                }

                Thread.Sleep(1000 * 60);//should sleep for 1 min, 10*60 for test only

            }
        }
        #endregion
    }
}
