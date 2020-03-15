using System.Collections.Generic;

namespace Splendor.Core
{
    public interface IPool
    {
        int this[TokenColour index] { get; }
        Pool CreateCopy();
        Pool DeficitFor(IPool other);
        Pool MergeWith(IPool other);

        /// <summary>
        /// Returns the colours with non-zero values.
        /// </summary>
        IEnumerable<TokenColour> Colours(bool includeGold = true);
        int Sum { get; }
        int Gold { get; }
        int White { get; }
        int Blue { get; }
        int Red { get; }
        int Green { get; }
        int Black { get; }
    }
}