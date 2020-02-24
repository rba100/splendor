using Splendor.Core.Actions;
using Splendor.Core.AI;
using System;

namespace Splendor.TestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var ai = new StupidSplendorAi();

            var gameState = new DefaultGameInitialiser(new DefaultCards()).Create(players: 2);
            
            var engine = new GameEngine(gameState);
            var playersSkippedInARow = 0;
            while (!engine.IsGameFinished && playersSkippedInARow < 2)
            {
                var action = ai.ChooseAction(gameState);
                Console.WriteLine($"Turn: {gameState.CurrentPlayer.Name} ({gameState.CurrentPlayer.VictoryPoints()}), Action: {action}");
                if (action is NoAction) playersSkippedInARow++; else playersSkippedInARow = 0;
                action.Execute(engine);
            }

            Console.WriteLine("Game over!");

            foreach(var player in gameState.Players)
            {
                Console.WriteLine($"{player.Name}: {player.VictoryPoints()}");
            }

            Console.ReadLine();
        }
    }
}
