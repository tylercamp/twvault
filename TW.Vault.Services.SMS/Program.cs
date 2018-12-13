using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.Services.SMS
{
    class Program
    {
        static CancellationToken ExitToken;
        static ILogger logger;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration.Instance).CreateLogger();
            logger = Log.ForContext<Program>();

            logger.Information("Initializing exit watcher");

            InitExitWatcher();

            int refreshDelay = Configuration.Behavior.Notifications.NotificationCheckInterval;
            var refreshDelayTimeSpan = TimeSpan.FromMilliseconds(refreshDelay);

            logger.Information("Creating notifications service");
            var notificationsService = new NotificationsService();

            logger.Information("Clearing expired notifications from first run");
            using (var context = Scaffold.VaultContext.MakeFromConfig())
                notificationsService.ClearExpiredNotifications(context);

            logger.Information("Starting notifications service loop at {ms}ms intervals", refreshDelay);
            while (!ExitToken.IsCancellationRequested)
            {
                using (var context = Scaffold.VaultContext.MakeFromConfig())
                    notificationsService.RunOnce(context, refreshDelayTimeSpan);
                Task.Delay(refreshDelay).Wait(ExitToken);
            }
            logger.Information("Exiting");
            Log.CloseAndFlush();
        }

        private static void InitExitWatcher()
        {
            var tokenSource = new CancellationTokenSource();
            ExitToken = tokenSource.Token;

            Action<String> signalExit = (String source) =>
            {
                logger.Information("Received ProcessExit event via " + source + ", setting cancellation token");
                tokenSource.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) => signalExit("ProcessExit");
            Console.CancelKeyPress += (sender, e) => signalExit("CancelKeyPress");
            AssemblyLoadContext.Default.Unloading += (ctx) => signalExit("Default.Unloading");
            AssemblyLoadContext.GetLoadContext(typeof(Program).GetTypeInfo().Assembly).Unloading += (ctx) => signalExit("GetLoadContext.Unloading");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                logger.Error(e.ExceptionObject as Exception, "An unhandled exception occurred");
            };
        }
    }
}
