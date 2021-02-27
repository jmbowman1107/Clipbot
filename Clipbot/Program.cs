using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Clipbot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            var appSettings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
            var serviceCollection = new ServiceCollection();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace).AddConsole( );
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            serviceCollection.AddLogging(l => l.AddConsole()).AddTransient(p => p.ResolveWith<ClipPosterService>(appSettings, logger));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var clipPoster = serviceProvider.GetService<ClipPosterService>();


            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            //var clipPoster = new ClipPoster(appConfig, _logger);



            //var backgroundTask = Task.Run(async () =>
            while (!token.IsCancellationRequested)
            {
                await clipPoster.PostNewClips();
                await Task.Delay(appSettings.ClipPollCycle * 1000, token);
            }
            //}, token);

            Console.ReadKey();
            tokenSource.Cancel();

        }

        private static void ConfigureServices(IServiceCollection services, ApplicationSettings appSettings)
        {
            services.AddLogging(configure => configure.AddConsole()).AddSingleton(a => ActivatorUtilities.CreateInstance<ApplicationSettings>(a, appSettings));
        }


    }

    static class ServiceProviderExtensions
    {
        public static T ResolveWith<T>(this IServiceProvider provider, params object[] parameters) where T : class =>
            ActivatorUtilities.CreateInstance<T>(provider, parameters);
    }
}