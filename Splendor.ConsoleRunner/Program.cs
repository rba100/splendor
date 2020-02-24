using Splendor.Core.AI;
using System;

namespace Splendor.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var aiPlayers = new[]
            {
                new StupidSplendorAi("Robin"),
                new StupidSplendorAi("James"),
                new StupidSplendorAi("Mat"),
            };

            var runner = new AiGameRunner(aiPlayers, Console.WriteLine);

            runner.Run();

            Console.ReadLine();
        }
    }
}
