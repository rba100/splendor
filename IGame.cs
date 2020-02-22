namespace Splendor.Core
{
    public interface IGame
    {
        bool IsGameFinished { get; }
        GameState State { get; }
        public int RoundsCompleted { get; }
        void CommitTurn();
    }
}
