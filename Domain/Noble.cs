using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class Noble
    {
        internal Noble(int victoryPoints, IReadOnlyDictionary<CoinColour, int> cost, string name = null)
        {
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            VictoryPoints = victoryPoints;
            Name = name ?? AutoName();
        }

        public IReadOnlyDictionary<CoinColour, int> Cost { get; private set; }
        public int VictoryPoints { get; private set; }

        public string Name { get; private set; }

        private string AutoName()
        {
            var colours = Cost.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key);
            return string.Join("", colours.Select(FromCoinColour));
        }

        private string FromCoinColour(CoinColour col)
        {
            switch (col)
            {
                case CoinColour.White: return "W";
                case CoinColour.Blue: return "U";
                case CoinColour.Red: return "R";
                case CoinColour.Green: return "G";
                case CoinColour.Black: return "B";
                case CoinColour.Gold: return "G";
                default: throw new ArgumentOutOfRangeException(nameof(col));
            }
        }
    }
}
