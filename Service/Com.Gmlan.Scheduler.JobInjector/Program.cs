using Autofac;
using Com.Gmlan.Core.Scheduler;
using System.Threading;
using Topshelf;

namespace Com.Gmlan.Scheduler.JobInjector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var builder = new ContainerBuilder();
            /*
            Database.SetInitializer<EfContext>(null);


            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            log4net.GlobalContext.Properties["application"] = applicationName;
            var log4NetConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["log4net.Config"]);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigPath));

            

            //Log4Net
            builder.RegisterModule(new LoggingModule());

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


            //Generic Service
            new List<Type>
            {
               typeof(EfRepository<>),
               typeof(GenericBusinessServices<>)
            }.ForEach(t => builder.RegisterGeneric(t).AsImplementedInterfaces().InstancePerLifetimeScope());

            //Common Service
            new List<Type>
            {
                typeof(MemoryCacheManager),
                typeof(SettingService),
                typeof(TaskService),
            }.ForEach(t => builder.RegisterType(t).AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope());

            */
            var container = builder.Build();
            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<ISchedulerService<CancellationToken>>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(
                        () =>
                            container.Resolve<ISchedulerService<CancellationToken>>());
                    serviceConfigurator.WhenStarted(myService => myService.Start());
                    serviceConfigurator.WhenStopped(myService => myService.Stop());
                });

                hostConfigurator.RunAsLocalSystem();
                hostConfigurator.SetDescription("Com.Gmlan.Scheduler Job injection service for Com.Gmlan.Scheduler component");
                hostConfigurator.SetDisplayName("Com.Gmlan.Scheduler Job injection Service");
                hostConfigurator.SetServiceName("Com.Gmlan.Scheduler.JobInjector");
            });
        }
    }
}