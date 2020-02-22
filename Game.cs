
using System;
using System.Linq;

using Splendor.Core.Actions;

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
        public GameState State { get; private set; }
        public bool IsGameFinished { get; private set; }
        public int RoundsCompleted { get; private set; }

        public Game(GameState game)
        {
            State = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void CommitTurn(IAction action)
        {
            // Run action
            State = action.Execute(State);

            // Do nobles
            AssignNobles();

            // Identify end game
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

            foreach (var noble in State.Nobles) 
            {
                bool ruledOut = false;
                foreach(var colour in noble.Cost.Keys)
                {
                    if (!currentPlayerBonuses.ContainsKey(colour) || noble.Cost[colour] > currentPlayerBonuses[colour])
                    {
                        ruledOut = true; break;
                    }
                }
                if (ruledOut) continue;

                // Give that noble to the player
                State.CurrentPlayer.Nobles.Add(noble);
                State.Nobles.Remove(noble);
                break;
            }
        }
    }
}
