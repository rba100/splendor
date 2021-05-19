
using System;
using System.Linq;

namespace Splendor.Core
{
    public record Noble
    {
        internal Noble(int victoryPoints, IPool cost, string name = null)
        {
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            VictoryPoints = victoryPoints;
            Name = name ?? AutoName();
        }

        public IPool Cost { get; init; }
        public int VictoryPoints { get; init; }
        public string Name { get; init; }

        private string AutoName()
        {
            return string.Join("", Cost.Colours().Select(FromCoinColour));
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
