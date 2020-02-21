using System;
using System.Collections.Generic;

namespace Splendor
{
    public class Noble
    {
        public Noble(int victoryPoints, IReadOnlyDictionary<CoinColour, int> cost)
        {
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            VictoryPoints = victoryPoints;
        }

        public IReadOnlyDictionary<CoinColour, int> Cost { get; private set; }
        public int VictoryPoints { get; private set; }
    }
}
