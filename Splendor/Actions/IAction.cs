namespace Splendor.Core.Actions
{
    public interface IAction
    {
        GameState Execute(GameState gameState);
    }
}
