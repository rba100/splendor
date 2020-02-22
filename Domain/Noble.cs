using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class Noble
    {
        internal Noble(int victoryPoints, IReadOnlyDictionary<TokenColour, int> cost, string name = null)
        {
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            VictoryPoints = victoryPoints;
            Name = name ?? AutoName();
        }

        public IReadOnlyDictionary<TokenColour, int> Cost { get; private set; }
        public int VictoryPoints { get; private set; }

        public string Name { get; private set; }

        private string AutoName()
        {
            var colours = Cost.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key);
            return string.Join("", colours.Select(FromCoinColour));
        }

        private string FromCoinColour(TokenColour col)
        {
            switch (col)
            {
                case TokenColour.White: return "W";
                case TokenColour.Blue: return "U";
                case TokenColour.Red: return "R";
                case TokenColour.Green: return "G";
                case TokenColour.Black: return "B";
                case TokenColour.Gold: return "G";
                default: throw new ArgumentOutOfRangeException(nameof(col));
            }
        }
    }
}
