using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using JeffBot;

namespace JeffBotWeb
{
    public class Program
    {
        public static List<JeffBot.StreamerSettings> StreamerSettings => new List<StreamerSettings> 
        { 
            // TODO: Add your streamers settings here
        };
        
        public static void Main(string[] args) 
        {
            foreach (var streamer in StreamerSettings)
            {
                var runMe = new JeffBot.JeffBot(streamer);
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}