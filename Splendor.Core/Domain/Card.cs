
using System;
using System.Linq;

namespace Splendor.Core
{
    public record Card
    {
        public int Tier { get; init; }
        public int VictoryPoints { get; init; }
        public IPool Cost { get; init; }
        public TokenColour BonusGiven { get; init; }

        public Card(int tier, int victoryPoints, IPool cost, TokenColour givesDiscount)
        {
            Tier = tier;
            VictoryPoints = victoryPoints;
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            BonusGiven = givesDiscount;
        }

        public override string ToString()
        {
            var tierMarker = new string('·', Tier);
            var costs = Cost.Colours().Select(col => $"{Cost[col]} {col}").ToList();
            return $"{VictoryPoints}pt {BonusGiven}{tierMarker} {string.Join(",", costs)}";
        }
    }
}
