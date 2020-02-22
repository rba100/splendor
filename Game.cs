
using System;
using System.Linq;

namespace Splendor.Core
{
    /// <summary>
    /// Logic for global game rules other than initialisation.
    /// </summary>
    /// <remarks>
    /// - Identifies when game has ended
    /// - Assigns nobles
    /// </remarks>
    public class Game : IGame
    {
        public GameState State { get; }
        public bool IsGameFinished { get; private set; }
        public int RoundsCompleted { get; private set; }

        public Game(GameState game)
        {
            State = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void CommitTurn()
        {
            // Do nobles
            AssignNobles();

            var endOfRound = State.Players.Last() == State.CurrentPlayer;
            if (endOfRound) RoundsCompleted++;
            IsGameFinished = endOfRound && State.Players.Any(p => p.VictoryPoints() >= 15);

            // Increment player
            var nextIndex = (Array.IndexOf(State.Players, State.CurrentPlayer) + 1)
                % State.Players.Length;
            State.CurrentPlayer = State.Players[nextIndex];
        }

        /// <summary>
        /// Player gets to choose which noble if there's more than once applicable,
        /// however this implementation just assigns the first one found.
        /// </summary>
        private void AssignNobles()
        {
            var currentPlayerBonuses = State.CurrentPlayer.GetDiscount();

            foreach (var nobleIndex in State.NobleTier.ColumnToFreeNobles.Keys.ToArray()) 
            {
                Noble noble = State.NobleTier.ColumnToFreeNobles[nobleIndex];
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
                State.CurrentPlayer.Nobles.Add(noble);
                // Remove noble from available
                State.NobleTier.ColumnToFreeNobles[nobleIndex] = null;
                break;
            }
        }
    }
}
