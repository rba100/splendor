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
        Player TopPlayer { get; }
     }
}
