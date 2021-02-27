using System;
using System.Collections.Generic;
using System.IO;

namespace Clipbot
{
    public static class SettingsHelpers
    {
        #region AddOrUpdateAppSetting
        public static void AddOrUpdateAppSetting<T>(T value)
        {
            try
            {
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
                throw new Exception($"Error writing app settings | {ex.Message}", ex);
            }
        } 
        #endregion
    }
}