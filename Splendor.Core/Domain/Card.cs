
using System;
using System.Linq;

namespace Splendor.Core
{
    public class Card
    {
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

        public int Tier { get; private set; }
        public int VictoryPoints { get; private set; }
        public IPool Cost { get; private set; }
        public TokenColour BonusGiven { get; private set; }
    }
}
