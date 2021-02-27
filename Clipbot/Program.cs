using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clipbot
{
    class Program
    {
        #region Main
        private static async Task Main(string[] args)
        {
            var appSettings = ConfigureServices(out var serviceProvider);
            var clipPoster = serviceProvider.GetService<ClipPosterService>();

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var backgroundTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await clipPoster.PostNewClips();
                    await Task.Delay(appSettings.ClipPollCycle * 1000, token);
                }
            }, token);

            Console.ReadKey();
            tokenSource.Cancel();
        } 
        #endregion

        #region ConfigureServices
        private static ApplicationSettings ConfigureServices(out ServiceProvider serviceProvider)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            var appSettings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
            var serviceCollection = new ServiceCollection();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(appSettings.LogLevel).AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            serviceCollection.AddLogging(l => l.AddConsole()).AddTransient(p => ActivatorUtilities.CreateInstance<ClipPosterService>(p, appSettings, logger));
            serviceProvider = serviceCollection.BuildServiceProvider();
            return appSettings;
        } 
        #endregion
    }
}