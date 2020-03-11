
using System;
using System.Linq;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.Blazor
{
    internal sealed class AutoReturnTakeTokens : IAction
    {
        public AutoReturnTakeTokens(IPool tokensToTake)
        {
            m_TokensToTake = tokensToTake
                ?? throw new ArgumentNullException(nameof(tokensToTake));
        }

        public GameState Execute(GameState gameState)
        {
            if (gameState is null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            var purse = gameState.CurrentPlayer.Purse;

            var tokensToReturn = new Pool();

            var colourPriority = purse
                .Colours(includeGold: false)
                .Except(m_TokensToTake.Colours())
                .SelectMany(c => Enumerable.Repeat(c, purse[c]))
                .ToList();

            colourPriority.Shuffle();

            colourPriority.AddRange
                (purse.Colours(includeGold: false)
                      .Where(c => m_TokensToTake.Colours().Contains(c))
                      .SelectMany(c => Enumerable.Repeat(c, purse[c])));

            var returnCount = Math.Max(purse.Sum + m_TokensToTake.Sum - 10, 0);

            foreach (var colour in colourPriority.Take(returnCount))
            {
                tokensToReturn[colour] += 1;
            }

            var action = new TakeTokens(m_TokensToTake, tokensToReturn);

            return action.Execute(gameState);
        }

        private readonly IPool m_TokensToTake;
    }
}
