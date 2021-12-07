using System;
using System.Threading.Tasks;

namespace BanHateBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bot = new BanHateBot();
            //await bot.GetRecentFollowersAndBanHate();
            Console.ReadLine();
        }
    }
}