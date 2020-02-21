using Splendor.Core.Actions;

using System;

namespace Splendor.Core.AI
{

    public class StupidSplendorAi : ISpendorAi
    {
        public IAction ChooseAction(GameState gamestate)
        {
            var me = gamestate.CurrentPlayer;

            // If I can buy a card in hand, do so
            throw new NotImplementedException();
        }
    }
}
