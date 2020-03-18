
using System;
using System.Linq;

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
        public string Name { get; } = $"{Guid.NewGuid()}";

        /// <remarks>Instances of Random must not be shared
        ///          if there is any chance of concurrency</remarks>
        ///
        public Random Random { private get; set; } = new Random();

        public IAction ChooseAction(GameState gameState)
        {
            var actionVariations = m_ActionEnumerator
                                  .GenerateValidActionVariations(gameState)
                                  .ToList();

            while (actionVariations.Any())
            {
                var index = Random.Next(actionVariations.Count);

                var actions = actionVariations[index].ToArray();

                if (actions.Any())
                {
                    return actions[Random.Next(actions.Length)];
                }
                
                actionVariations.RemoveAt(index);
            }

            return new NoAction();
        }

        private IActionEnumerator m_ActionEnumerator =
            new CompositeActionEnumerator(new TakeTokensActionEnumerator(),
                                          new BuyCardActionEnumerator(),
                                          new ReserveCardActionEnumerator());
    }
}