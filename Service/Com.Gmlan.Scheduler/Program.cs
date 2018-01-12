using Autofac;
using Com.Gmlan.Core.Cache;
using Com.Gmlan.Core.Helper;
using Com.Gmlan.Core.Scheduler;
using Com.Gmlan.Core.System;
using Com.Gmlan.Middlewares.MessageQueue.Event;
using Com.Gmlan.Middlewares.MessageQueue.Model.Generic;
using Com.Gmlan.Middlewares.MongoDB.Data;
using Com.Gmlan.Middlewares.MongoDB.Model;
using Com.Gmlan.Middlewares.MongoDB.Service;
using Com.Gmlan.Scheduler.Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using Topshelf;

namespace Com.Gmlan.Scheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            int maxFetchCount;
            if (!int.TryParse(ConfigurationManager.AppSettings["RabbitMq_Max_Fetch_Count"], out maxFetchCount))
                maxFetchCount = 5;

            var builder = new ContainerBuilder();

            /*
            Database.SetInitializer<EfContext>(null);


            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            log4net.GlobalContext.Properties["application"] = applicationName;
            var log4NetConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["log4net.Config"]);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigPath));


            //Log4Net
            builder.RegisterModule(new LoggingModule());

            //core layer
            builder.RegisterType<RedisCacheManager>()
                .AsSelf()
                .SingleInstance();

            //data layer
            builder.RegisterType<EfContext>()
                .AsImplementedInterfaces()
                .AsSelf()
                .As<DbContext>()
                .InstancePerLifetimeScope();

            //Service Layer
            builder.RegisterType<LogInterceptor>();

            builder.RegisterType<SchedulerService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(LogInterceptor));
            */

            //Generic Service
            new List<Type>
            {
               typeof(MongoBusinessService<>),
               typeof(MongoRepository<>)
            }.ForEach(t => builder.RegisterGeneric(t).AsImplementedInterfaces().InstancePerLifetimeScope());

            //Common Service
            new List<Type>
            {
                typeof(JobExecutor),
            }.ForEach(t => builder.RegisterType(t).AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope());


            ////register registries in all plugins loaded
            ////  AssemblyHelper.ReadPluginAssemblies("Framework", "Framework");
            //AssemblyHelper.ReadPluginAssemblies("Plugins", "Plugin");
            //var allRegistries = AssemblyHelper.FindAllConcreteTypesFromPlugin(typeof(IDependencyRegistry));
            //foreach (var registryType in allRegistries)
            //{
            //    var registry = (IDependencyRegistry)Activator.CreateInstance(registryType);
            //    registry.Register(builder);
            //}

            AssemblyHelper.ReadPluginAssemblies("Hooks", "Com.Gmlan.Scheduler.Hooks.");
            foreach (var type in AssemblyHelper.FindAllConcreteTypesFromPlugin(typeof(IJobHook)))
            {
                builder.RegisterType(type)
                    .UsingConstructor(new ParameterlessConstructorSelector())
                    .Named<IJobHook>(type.FullName)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }

            builder.Register(c => new MongoDbConfig()).As<MongoDbConfig>().SingleInstance();
            var container = builder.Build();

            //Get singleton MongoDbConfig object and update with value from DB
            var mongoDbConfig = container.Resolve<MongoDbConfig>();
            ISettingService settingService = container.Resolve<ISettingService>();
            mongoDbConfig.ConnectionString =
                settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_MONGODB_CONNECTIONSTRING);
            mongoDbConfig.Database =
                settingService.GetSettingValueByKey(Constant.SYSTEM_INFRASTRUCTURE_MONGODB_DEFAULT_DATABASE);

            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<ISchedulerService<ReceivedEventArgs<MessageBody>>>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(
                        settings =>
                            container.Resolve<ISchedulerService<ReceivedEventArgs<MessageBody>>>(
                                new NamedParameter("cacheManager", container.Resolve(typeof(ICacheManager))),
                                new NamedParameter("consumerQueue", settings.InstanceName),
                                new NamedParameter("maxFetchCount", maxFetchCount)));
                    serviceConfigurator.WhenStarted(myService => myService.Start());
                    serviceConfigurator.WhenStopped(myService => myService.Stop());
                });

                hostConfigurator.RunAsLocalSystem();
                hostConfigurator.SetDescription("Com.Gmlan Scheduler Service as a backend service to service the whole system.");
                hostConfigurator.SetDisplayName("Com.Gmlan Scheduler Service");
                hostConfigurator.SetServiceName("Com.Gmlan.Scheduler");
            });
        }
    }
}