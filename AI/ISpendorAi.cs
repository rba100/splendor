using Splendor.Core.Actions;

namespace Splendor.Core.AI
{
    public interface ISpendorAi
    {
        IAction ChooseAction(GameState gamestate);
    }
}
