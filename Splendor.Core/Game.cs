
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.Actions;

namespace Splendor.Core
{
    /// <summary>
    /// Logic for global game rules.
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

        private const int c_PointsForWin = 15;

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
            var endOfRound = State.Players.Last().Name == State.CurrentPlayer.Name;
            if (endOfRound) RoundsCompleted++;
            IsGameFinished = endOfRound && State.Players.Any(p => p.VictoryPoints >= c_PointsForWin);

            // Increment player
            State = State.CloneWithIncrementedCurrentPlayer();
        }

        public Player TopPlayer
        {
            get
            {
                return State.Players.OrderByDescending(p => p.VictoryPoints)
                                    .ThenByDescending(p => p.CardsInPlay.Count).First();
            }
        }

        /// <summary>
        /// Player gets to choose which noble if there's more than once applicable,
        /// however this implementation just assigns the first one found.
        /// </summary>
        private void AssignNobles()
        {
            var currentPlayerBonuses = State.CurrentPlayer.Bonuses;
            if (currentPlayerBonuses.SumValues() < 8) return;

            var nextNobles = new List<Noble>(State.Nobles);
            foreach (var noble in State.Nobles)
            {
                bool ruledOut = false;
                foreach (var colour in noble.Cost.Colours())
                {
                    if (!currentPlayerBonuses.ContainsKey(colour) || noble.Cost[colour] > currentPlayerBonuses[colour])
                    {
                        ruledOut = true; break;
                    }
                }
                if (ruledOut) continue;

                var nextPlayer = State.CurrentPlayer.CloneWithNoble(noble);
                nextNobles.Remove(noble);

                State = State.Clone(withNobles: nextNobles).CloneWithPlayerReplacedByName(nextPlayer);
                break;
            }
        }
    }
}
