namespace Splendor.Core.Actions
{
    /// <summary>
    /// Skips turn. Game ends if all players skip their turns in a round.
    /// </remarks>
    public class NoAction : IAction
    {
        public void Execute(GameEngine gameEngine)
        {
            gameEngine.CommitTurn();
        }
    }
}
