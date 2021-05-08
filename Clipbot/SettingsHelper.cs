using System;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Clipbot
{
    public static class SettingsHelpers
    {
        #region AddOrUpdateAppSetting
        public static void AddOrUpdateAppSetting<T>(T value, ILogger logger, bool isDynamoDb)
        {
            if (!isDynamoDb)
            {
                try
                {
                    logger.LogTrace($"Saving updated settings to config file");
                    var settingFiles = new List<string> { "appsettings.json" };
                    foreach (var item in settingFiles)
                    {
                        var filePath = Path.Combine(AppContext.BaseDirectory, item);
                        var output = Newtonsoft.Json.JsonConvert.SerializeObject(new { ApplicationSettings = value }, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(filePath, output);
                    }
                }
                catch (Exception ex)
                {
                    UpdateSettingsInDynamoDb(value);
                    logger.LogTrace($"Error saving config file", ex.ToString());
                    throw new Exception($"Error writing app settings | {ex.Message}", ex);
                }
            }
            else
            {
                UpdateSettingsInDynamoDb(value);
            }
        }

        private static void UpdateSettingsInDynamoDb<T>(T value)
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            var client = new AmazonDynamoDBClient(dynamoDbConfig);

            client.UpdateItemAsync(new UpdateItemRequest("ClipbotSettings", new Dictionary<string, AttributeValue> {{"BroadcasterID", new AttributeValue("75230612")}},
                new Dictionary<string, AttributeValueUpdate>
                {
                    {
                        "Settings",
                        new AttributeValueUpdate(new AttributeValue(JsonConvert.SerializeObject(value)), AttributeAction.PUT)
                    }
                })).Wait();
        }

        #endregion
    }
}