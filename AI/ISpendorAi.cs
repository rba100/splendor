using Splendor.Core.Actions;

namespace Splendor.Core.AI
{
    public interface ISpendorAi
    {
        string Name { get; }
        IAction ChooseAction(GameState gameState);
    }
}
