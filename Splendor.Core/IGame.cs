using Splendor.Core.Actions;

namespace Splendor.Core
{
    public interface IGame
    {
        bool IsGameFinished { get; }
        GameState State { get; }
        void CommitTurn(IAction action);

        // Metrics
        int RoundsCompleted { get; }

        /// <summary>
        /// The player with the most prestige points. If there's a tie, ranking is then by the fewest development cards.
        /// </summary>
        Player TopPlayer { get; }
     }
}
