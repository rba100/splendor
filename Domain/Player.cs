using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor
{
    public class Player
    {
        public string Name { get; }

        public Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Purse = Utility.CreatePurse();
            Nobles = new List<Noble>();
            ReservedCards = new List<Card>();
            CardsInPlay = new List<Card>();
        }

        public IDictionary<CoinColour, int> Purse { get; private set; }
        public IList<Card> ReservedCards { get; private set; }
        public IList<Card> CardsInPlay { get; private set; }
        public IList<Noble> Nobles { get; private set; }

        public IDictionary<CoinColour, int> GetDiscount()
        {
            return Enum.GetValues(typeof(CoinColour))
                       .OfType<CoinColour>()
                       .ToDictionary(col => col, col => CardsInPlay.Count(c => c.GivesDiscount == col));
        }

        public int VictoryPoints()
        {
            return CardsInPlay
                .Select(n => n.VictoryPoints)
                .Concat(Nobles.Select(c => c.VictoryPoints))
                .Sum();
        }
    }
}
