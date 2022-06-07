
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;
using Splendor.Core.AI;

namespace Splendor.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //Interactive();
            //RunDetailed();
            Run(numberOfGames: 2000, useParallelism: true);
        }

        static void Interactive()
        {
            var runner = new InteractiveRunner();
            runner.Run();
            Console.ReadLine();
        }

        static void RunDetailed()
        {
            var aiOptions = new AiOptions{};
            var aiPlayers = new ISpendorAi[]
            {
                new ObservantStupidSplendorAi("Stupid1"),
                new ObservantStupidSplendorAi("Observant",aiOptions),
                new ObservantStupidSplendorAi("Stupid2"),
            };

            var runner = new AiGameRunner(aiPlayers, Console.WriteLine);

            runner.Run();

            if(Debugger.IsAttached)
                Console.ReadLine();
        }

        static void Run(int numberOfGames = 1, bool useParallelism = true)
        {
            int tenPercent = numberOfGames / 10;

            var aiOptions = new AiOptions { };
            var droneOptions = new AiOptions { };

            var scoreBoard = new Dictionary<ISpendorAi, int>
            {
                { new ObservantStupidSplendorAi("Test Subject", aiOptions), 0},
                //{ new ObservantStupidSplendorAi("Drone1", droneOptions), 0},
                { new StupidSplendorAi("Drone2"), 0},
            };
            var ais = scoreBoard.Select(r => r.Key).ToArray();
            var range = Enumerable.Range(0, numberOfGames);

            Dictionary<ISpendorAi,int> runGame(int iteration) {
                if (iteration % tenPercent == 0) Console.Write(".");
                var a = ais.ToList();
                a.Shuffle();
                return new AiGameRunner(a, _ => { }).Run();
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

            if(Debugger.IsAttached)
                Console.ReadLine();
        }
    }
}
