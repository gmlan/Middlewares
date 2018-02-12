using Autofac;
using Com.Gmlan.Core.Cache;
using Com.Gmlan.Core.Resolver;
using Com.Gmlan.Core.Scheduler;
using Com.Gmlan.Core.System;
using Com.Gmlan.Middlewares.MessageQueue.Client.Generic;
using Com.Gmlan.Middlewares.MessageQueue.Event;
using Com.Gmlan.Middlewares.MessageQueue.Model;
using Com.Gmlan.Middlewares.MessageQueue.Model.Generic;
using Com.Gmlan.Scheduler.Model;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;
using Com.Gmlan.Middlewares.MessageQueue.Extension;

namespace Com.Gmlan.Scheduler
{
    public class SchedulerService : ISchedulerService<ReceivedEventArgs<MessageBody>>
    {

        #region fields

        private readonly ILog _log;
        private readonly ICacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly string _consumerQueue;
        private readonly int _maxFetchCount;
        private AssemblyResolver _assemblyResolver;
        private readonly ILifetimeScope _lifetimeScope;

        private RabbitMqReceiver<MessageBody> _receiver;
        #endregion

        public SchedulerService(ILifetimeScope lifetimeScope, ILog log, ICacheManager cacheManager, ISettingService settingService, string consumerQueue, int maxFetchCount)
        {
            _lifetimeScope = lifetimeScope;
            _log = log;
            _cacheManager = cacheManager;
            _settingService = settingService;
            _consumerQueue = consumerQueue;
            _maxFetchCount = maxFetchCount;
        }

        public void Start()
        {
            _assemblyResolver = new AssemblyResolver(_log);
            _assemblyResolver.Attach();

            _receiver = new RabbitMqReceiver<MessageBody>(
                _settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_RABBITMQ_CONNECTIONSTRING),
                _consumerQueue, false, (ushort)_maxFetchCount);

            _receiver.SubscribeEvents(message => { _log.Info($"{_consumerQueue}:{message}"); });
            _receiver.Received += (model, ea) =>
            {
                Task.Run(() => Run(ea));
            };

            _receiver.Start();


            //Start heartbeat
            new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        //cache 1 min then sleep 30 secs
                        _cacheManager.Set(HostInfo.MachineKey, new CacheEntity<int> { CreateTimeUtc = DateTime.UtcNow, Value = 1 }, 1);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _log.Error(ex.Message, ex);
                        }
                        catch { }//For Log exception, nothing to catch so that the log exception won't break the task logic.
                    }
                    finally
                    {
                        Thread.Sleep(1000 * 30);
                    }
                }
            }).Start();

            _log.Info("Service started");
        }

        public void Stop()
        {
            try
            {
                _assemblyResolver?.Detach();
                _receiver?.Dispose();

                //Remove all cached since current process is going to stop
                //TODO:3.0: Current Max 60 mins
            }
            catch (System.Exception ex)
            {
                _log.Error($"Exception thrown when try to stop the service : {ex.Message}", ex);
            }

            _log.Info("Service stopped");
        }

        #region Methods.

        public void Run(ReceivedEventArgs<MessageBody> args)
        {
            var message = args.Value;
            var deliverEventArgs = args.DeliverEventArgs;
            var receiver = args.Receiver;
            try
            {
                var param = message.DeserializeAsParam();
                using (var scope = _lifetimeScope.BeginLifetimeScope())
                {
                    var type = Type.GetType(message.ServiceTypeFullName);
                    if (type != null)
                    {
                        var mi = type.GetMethod(message.ServiceMethodName);
                        var service = scope.Resolve(type);
                        if (mi != null)
                            mi.Invoke(service,
                                new object[] { new QueueMessageModel { Data = param, QueueName = _consumerQueue } });
                        else
                            throw new MissingMethodException(
                                $"Can not find method in type {message.ServiceTypeFullName}");
                    }
                    else
                    {
                        throw new TypeLoadException($"Can not find type {message.ServiceTypeFullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _log.Error(ex.Message, ex);
                }
                catch { }//For Log exception, nothing to catch so that the log exception won't break the task logic.
            }
            finally
            {
                receiver.Ack(deliverEventArgs.DeliveryTag, false);
            }
        }
        #endregion
    }
}
