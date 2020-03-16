
using System;
using System.Collections.Generic;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.AIs.Unofficial.Actions
{
    public sealed class ReserveCardActionEnumerator : IActionEnumerator
    {
        public IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState)
        {
            if (gameState is null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            if (gameState.CurrentPlayer.ReservedCards.Count >= 3)
            {
                return new IEnumerable<IAction>[] {};
            }

            return new IEnumerable<IAction>[] {};
        }
    }
}