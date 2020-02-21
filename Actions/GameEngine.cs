using System;
using System.Linq;

namespace Splendor.Core.Actions
{
    /// <summary>
    /// Logic for game rules other than initialisation
    /// </summary>
    public class GameEngine : IGameEngine
    {
        public GameState GameState { get; }
        public bool IsGameFinished { get; private set; }
        public int RoundsCompleted { get; private set; }

        public GameEngine(GameState game)
        {
            GameState = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void CommitTurn()
        {
            // Do nobles
            AssignNobles();

            var endOfRound = GameState.Players.Last() == GameState.CurrentPlayer;
            if (endOfRound) RoundsCompleted++;
            IsGameFinished = endOfRound && GameState.Players.Any(p => p.VictoryPoints() >= 15);

            // Increment player
            var nextIndex = (Array.IndexOf(GameState.Players, GameState.CurrentPlayer) + 1)
                % GameState.Players.Length;
            GameState.CurrentPlayer = GameState.Players[nextIndex];
        }

        /// <summary>
        /// Player gets to choose which noble if there's more than once applicable,
        /// however this implementation just assigns the first one found.
        /// </summary>
        private void AssignNobles()
        {
            var currentPlayerBonuses = GameState.CurrentPlayer.GetDiscount();

            foreach (var nobleIndex in GameState.NobleTier.ColumnToFreeNobles.Keys.ToArray()) 
            {
                Noble noble = GameState.NobleTier.ColumnToFreeNobles[nobleIndex];
                if (noble == null) continue;
                foreach(var colour in noble.Cost.Keys)
                {
                    if (!currentPlayerBonuses.ContainsKey(colour) || noble.Cost[colour] > currentPlayerBonuses[colour])
                    {
                        noble = null; break;
                    }
                }
                if (noble == null) continue;

                // Give that noble to the player
                GameState.CurrentPlayer.Nobles.Add(noble);
                // Remove noble from available
                GameState.NobleTier.ColumnToFreeNobles[nobleIndex] = null;
                break;
            }
        }
    }

    public interface IGameEngine
    {
        bool IsGameFinished { get; }
        GameState GameState { get; }
        public int RoundsCompleted { get; }
        void CommitTurn();
    }
}
