
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.AIs.Unofficial.Actions
{
    /// <remarks>Returns the groups from all underlying
    ///          enumerators. Does not combine groups</remarks>
    ///
    public sealed class CompositeActionEnumerator : IActionEnumerator
    {
        public CompositeActionEnumerator
            (params IActionEnumerator[] underlyingEnumerators)
        {
            m_UnderlyingEnumerators = underlyingEnumerators?.ToArray()
                ?? throw new ArgumentNullException(nameof(underlyingEnumerators));

            if (m_UnderlyingEnumerators.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(underlyingEnumerators));
            }
        }

        public IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState)
        {
            return m_UnderlyingEnumerators.SelectMany
                (e => e.GenerateValidActionVariations(gameState));
        }

        private readonly IReadOnlyCollection<IActionEnumerator>
            m_UnderlyingEnumerators;
    }
}