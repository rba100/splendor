
using System;

using Splendor.AIs.Unofficial.Actions;
using Splendor.Core;
using Splendor.Core.Actions;
using Splendor.Core.AI;

namespace Splendor.AIs.Unofficial
{
    /// <summary>Picks a random valid action variation
    ///          from a random group of valid actions</summary>
    ///
    public sealed class BogoSpendorAi : ISpendorAi
    {
        public string Name => nameof(BogoSpendorAi);

        /// <remarks>Instances of Random must not be shared
        ///          if there is any chance of concurrency</remarks>
        ///
        public Random Random { private get; set; } = new Random();

        public IActionEnumerator ActionEnumerator { private get; set; }

        public IAction ChooseAction(GameState gameState) =>
            ActionEnumerator.GenerateValidActionVariations(gameState)
                            .GetRandomItem(Random)
                            .GetRandomItem(Random);
    }
}