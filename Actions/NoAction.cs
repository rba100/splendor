namespace Splendor.Core.Actions
{
    /// <summary>
    /// Skips turn. Game ends if all players skip their turns in a round.
    /// </remarks>
    public class NoAction : IAction
    {
        public void Execute(IGameEngine gameEngine)
        {
            gameEngine.CommitTurn();
        }

        public override string ToString()
        {
            return $"Pass";
        }
    }
}
