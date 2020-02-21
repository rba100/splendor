using System;
using System.Collections.Generic;

namespace Splendor
{
    public class Card
    {
        public Card(int tier, int victoryPoints, IReadOnlyDictionary<CoinColour, int> cost, CoinColour givesDiscount)
        {
            Tier = tier;
            VictoryPoints = victoryPoints;
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            GivesDiscount = givesDiscount;
        }

        public int Tier { get; private set; }
        public int VictoryPoints { get; private set; }
        public IReadOnlyDictionary<CoinColour, int> Cost { get; private set; }
        public CoinColour GivesDiscount { get; private set; }
    }
}
