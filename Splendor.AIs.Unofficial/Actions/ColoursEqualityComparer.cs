
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor.AIs.Unofficial.Actions
{
    /// <remarks>Assumes the colours are already sorted
    ///          consistently in compared collections</remarks>
    ///
    internal sealed class ColoursEqualityComparer
        :
        IEqualityComparer<IEnumerable<TokenColour>>
    {
        public bool Equals(IEnumerable<TokenColour> a,
                           IEnumerable<TokenColour> b) => a.SequenceEqual(b);

        public int GetHashCode(IEnumerable<TokenColour> obj) =>
            unchecked(obj.Aggregate(17, (a, b) => 23 * a + (int)b));
    }
}