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
            BonusGiven = givesDiscount;
        }

        public override string ToString()
        {
            var tierMarker = new string('·', Tier);
            return $"{BonusGiven}{tierMarker} ({VictoryPoints})";
        }

        public int Tier { get; private set; }
        public int VictoryPoints { get; private set; }
        public IReadOnlyDictionary<CoinColour, int> Cost { get; private set; }
        public CoinColour BonusGiven { get; private set; }
    }
}
