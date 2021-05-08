using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.Core;
using Clipbot;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ClipbotLambda
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(ILambdaContext context)
        {
            var appSettings = ConfigureServices(out var serviceProvider);
            var clipPoster = serviceProvider.GetService<ClipPosterService>();
            await clipPoster.PostNewClips(true);
        }

        #region ConfigureServices
        private static ApplicationSettings ConfigureServices(out ServiceProvider serviceProvider)
        {
            try
            {
                var dynamoDbConfig = new AmazonDynamoDBConfig();
                dynamoDbConfig.RegionEndpoint = RegionEndpoint.USEast1;

                var client = new AmazonDynamoDBClient(dynamoDbConfig);
                var settings = client.GetItemAsync("ClipbotSettings", new Dictionary<string, AttributeValue> {{"BroadcasterID", new AttributeValue("75230612")}}).Result;

                ApplicationSettings appSettings;
                if (settings.Item.Count == 0)
                {
                    IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                    appSettings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
                }
                else
                {
                    appSettings = JsonConvert.DeserializeObject<ApplicationSettings>(settings.Item["Settings"].S);
                }
                var serviceCollection = new ServiceCollection();
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(appSettings.LogLevel).AddConsole();
                });
                ILogger logger = loggerFactory.CreateLogger<Function>();

                serviceCollection.AddLogging(l => l.AddConsole()).AddTransient(p => ActivatorUtilities.CreateInstance<ClipPosterService>(p, appSettings, logger));
                serviceProvider = serviceCollection.BuildServiceProvider();
                return appSettings;
            }
            // try to get file from s3, if not, use the default here..

            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
