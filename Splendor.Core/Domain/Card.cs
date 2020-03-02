
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class Card
    {
        internal Card(int tier, int victoryPoints, IReadOnlyDictionary<TokenColour, int> cost, TokenColour givesDiscount)
        {
            Tier = tier;
            VictoryPoints = victoryPoints;
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            BonusGiven = givesDiscount;
        }

        public override string ToString()
        {
            var tierMarker = new string('·', Tier);
            var costs = Cost.Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            return $"{VictoryPoints}pt {BonusGiven}{tierMarker} {string.Join(",", costs)}";
        }

        public int Tier { get; private set; }
        public int VictoryPoints { get; private set; }
        public IReadOnlyDictionary<TokenColour, int> Cost { get; private set; }
        public TokenColour BonusGiven { get; private set; }
    }
}
