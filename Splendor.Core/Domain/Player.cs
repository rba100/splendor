
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public record Player
    {
        public string Name { get; init; }
        public IPool Purse { get; init; }
        public IReadOnlyCollection<Card> ReservedCards { get; init; }
        public IReadOnlyCollection<Card> CardsInPlay { get; init; }
        public IReadOnlyCollection<Noble> Nobles { get; init; }

        public Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Purse = new Pool();
            Nobles = new List<Noble>();
            ReservedCards = new List<Card>();
            CardsInPlay = new List<Card>();
        }

        private Player(string name,
                       IPool purse,
                       IReadOnlyCollection<Card> reservedCards,
                       IReadOnlyCollection<Card> cardsInPlay,
                       IReadOnlyCollection<Noble> nobles)
        {
            Name = name;
            Purse = purse ?? throw new ArgumentNullException(nameof(purse));
            ReservedCards = reservedCards ?? throw new ArgumentNullException(nameof(reservedCards));
            CardsInPlay = cardsInPlay ?? throw new ArgumentNullException(nameof(cardsInPlay));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
        }

        public Player CloneWithNoble(Noble noble)
        {
            if (noble is null)
            {
                throw new ArgumentNullException(nameof(noble));
            }
            var nobles = new List<Noble>(Nobles);

            nobles.Add(noble);

            return this with { Nobles = nobles };
        }

        public IPool Budget { get => GetDiscount().MergeWith(Purse); }
        public IPool Bonuses { get => GetDiscount(); }
        public int VictoryPoints { get => GetVictoryPoints(); }

        private IPool GetDiscount()
        {
            var pool = new Pool();
            foreach (var c in CardsInPlay) pool[c.BonusGiven]++;
            return pool;
        }

        private int GetVictoryPoints()
        {
            return CardsInPlay
                .Select(n => n.VictoryPoints)
                .Concat(Nobles.Select(c => c.VictoryPoints))
                .Sum();
        }
    }
}
