namespace Splendor.Core.Actions
{
    public interface IGameEngine
    {
        bool IsGameFinished { get; }
        GameState GameState { get; }
        public int RoundsCompleted { get; }
        void CommitTurn();
    }
}
