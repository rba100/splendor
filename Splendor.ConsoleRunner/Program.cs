
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
            //Run1();
            Run(numberOfGames: 10000, useParallelism: true);
        }

        static void Run1()
        {
            var aiPlayers = new ISpendorAi[]
            {
                new NobleButStupidSplendorAi("Noble"),
                new ObservantStupidSplendorAi("Invest"),
                new StupidSplendorAi("Stupid"),
            };

            var runner = new AiGameRunner(aiPlayers, Console.WriteLine);

            runner.Run();

            Console.ReadLine();
        }

        static void Run(int numberOfGames = 1, bool useParallelism = true)
        {
            int tenPercent = numberOfGames / 10;

            var scoreBoard = new Dictionary<ISpendorAi, int>
            {
                { new ObservantStupidSplendorAi("Robin"), 0},
                { new StupidSplendorAi("James"), 0},
                { new NobleButStupidSplendorAi("Mat"), 0},
            };
            var ais = scoreBoard.Select(p => p.Key).ToArray();
            var range = Enumerable.Range(0, numberOfGames);

            Dictionary<ISpendorAi,int> runGame(int iteration) {
                if (iteration % tenPercent == 0) Console.Write(".");
                return new AiGameRunner(ais, _ => { }).Run();
            };

            var resultsSet = useParallelism
                ? range.AsParallel().Select(runGame).ToList()
                : range.Select(runGame).ToList();
            
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
