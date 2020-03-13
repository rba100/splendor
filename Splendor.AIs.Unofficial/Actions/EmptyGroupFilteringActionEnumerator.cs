
using System;
using System.Collections.Generic;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.AIs.Unofficial.Actions
{
    /// <summary>Lazily omits empty inner enumerations from results</summary>
    ///
    /// <remarks>
    /// Intended to be used within other action enumerator implementations
    /// to easily meet the requirements of the IActionEnumerator contract
    /// </remarks>
    /// 
    internal sealed class EmptyGroupFilteringActionEnumerator
        :
        IActionEnumerator
    {
        public EmptyGroupFilteringActionEnumerator
            (IActionEnumerator underlyingEnumerator)
        {
            m_UnderlyingEnumerator = underlyingEnumerator
                ?? throw new ArgumentNullException(nameof(underlyingEnumerator));
        }

        public IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState)
        {
            var actionVariations = m_UnderlyingEnumerator
                                  .GenerateValidActionVariations(gameState);

            foreach (var actionGroup in actionVariations)
            {
                var enumerator = actionGroup.GetEnumerator();

                IEnumerable<IAction> enumerable = null;

                try
                {
                    if (enumerator.MoveNext())
                    {
                        enumerable = NonEmptyEnumerate(enumerator);
                    }
                    else enumerator.Dispose();
                }
                catch { enumerator.Dispose(); throw; }

                if (enumerable != null) yield return enumerable;
            }
        }

        private static IEnumerable<IAction> NonEmptyEnumerate
            (IEnumerator<IAction> enumerator)
        {
            using (enumerator)
            {
                do
                {
                    yield return enumerator.Current;
                }
                while (enumerator.MoveNext());
            }
        }

        private readonly IActionEnumerator m_UnderlyingEnumerator;
    }
}