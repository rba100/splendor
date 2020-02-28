
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.AI;

namespace Splendor.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(numberOfGames: 1000);
        }

        static void Run1()
        {
            var aiPlayers = new ISpendorAi[]
            {
                new NobleButStupidSplendorAi("Robin"),
                new StupidSplendorAi("James"),
                new StupidSplendorAi("Mat"),
            };

            var runner = new AiGameRunner(aiPlayers, Console.WriteLine);

            runner.Run();

            Console.ReadLine();
        }

        static void Run(int numberOfGames)
        {
            int tenPercent = numberOfGames / 10;

            var scoreBoard = new Dictionary<ISpendorAi, int>
            {
                { new StupidSplendorAi("James"), 0},
                { new StupidSplendorAi("Robin"), 0},
                { new NobleButStupidSplendorAi("Mat"), 0},
            };
            var ais = scoreBoard.Select(p => p.Key).ToArray();

            var resultsSet = Enumerable.Range(0, numberOfGames)
                                       .AsParallel()
                                       .Select(i => { if (i % tenPercent == 0) Console.Write(".");
                                                      return new AiGameRunner(ais, _ => { }).Run(); })
                                       .ToList();
            
            foreach(var results in resultsSet)
            {
                var max = results.Values.Max();
                foreach (var ai in results.Where(r => r.Value == max)) scoreBoard[ai.Key]++;
            }

            Console.Write(Environment.NewLine);

            foreach (var row in scoreBoard)
            {
                Console.WriteLine($"{row.Key.Name} {row.Value} wins");
            }

            Console.ReadLine();
        }
    }
}
