using System;
using System.Collections.Generic;

namespace Splendor
{
    public class NobleTier
    {
        public IDictionary<int, Noble> ColumnToFreeNobles { get; private set; }
        public int StartingNobleCount { get; private set; }

        public NobleTier(ICollection<Noble> nobles)
        {
            if (nobles is null)
            {
                throw new ArgumentNullException(nameof(nobles));
            }

            StartingNobleCount = nobles.Count;

            ColumnToFreeNobles = new Dictionary<int, Noble>();

            int index = 0;
            foreach (var n in nobles) ColumnToFreeNobles[index++] = n;
        }
    }
}
