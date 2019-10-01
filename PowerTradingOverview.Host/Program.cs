using Autofac;
using log4net;
using log4net.Config;
using System;
using Topshelf;
using TradingPlatform;

namespace PowerTradingOverview.Host
{
    partial class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        static int Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure();

                var result = HostFactory.Run(configuration =>
                {
                    configuration.Service<JobScheduler>(service =>
                    {
                        service.ConstructUsing(() => CreateJobScheduler());
                        service.WhenStarted(x => x.Start());
                        service.WhenStopped(x => x.Stop());
                        service.WhenPaused(x => x.Stop());
                        service.WhenContinued(x => x.Start());
                    });

                    configuration.StartAutomatically();
                    configuration.SetDescription("Power Trading Overview Service");
                    configuration.SetDisplayName("Power Trading Overview Service");
                    configuration.SetServiceName("PowerTradingOverview");

                    configuration.UseLog4Net();
                });

                return (int)result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occured in Program.Main: ", ex);
                return 1;
            }
        }

        private static JobScheduler CreateJobScheduler()
        {
            return CreateContainer(ReportOptions.ReadFromConfig()).Resolve<JobScheduler>();
        }

        private static IContainer CreateContainer(ReportOptions reportOptions)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TimeZoneTimeProvider>()
                .WithParameter("timeZone", reportOptions.TimeZone)
                .As<ITimeProvider>()
                .SingleInstance();

            builder.Register(resolver => 
            {
                var startTime = resolver.Resolve<ITimeProvider>().GetCurrentTime();
                _logger.Debug($"Schedule start time is {startTime}");
                return new IntervalSchedule(startTime, reportOptions.Interval);
            }).As<IJobSchedule>();

            builder.RegisterType<TradingService>().AsSelf();
            builder.RegisterType<ReportBuilder>().As<IReportBuilder>();
            builder.RegisterType<CsvReportWriter>()
                .WithParameter("fileNamePattern", reportOptions.FilePattern)
                .WithParameter("directory", reportOptions.Directory)
                .As<IReportWriter>();

            builder.RegisterType<ReportExtractJob>().As<IJob>();

            builder.RegisterType<JobScheduler>().AsSelf();

            return builder.Build();
        }
    }
}
