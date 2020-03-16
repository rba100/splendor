
using System;
using System.Collections.Generic;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.AIs.Unofficial.Actions
{
    public sealed class BuyCardActionEnumerator : IActionEnumerator
    {
        public IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState)
        {
            if (gameState is null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            return new IEnumerable<IAction>[] {};
        }
    }
}