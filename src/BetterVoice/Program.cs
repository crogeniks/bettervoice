using System;
using System.Threading.Tasks;

namespace BetterVoice
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("BetterVoice starting!");

            var bot = new BetterVoice();

            await bot.StartAsync();
        }
    }
}
