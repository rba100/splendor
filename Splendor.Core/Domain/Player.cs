
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public class Player
    {
        public string Name { get; }

        internal Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Purse = new Pool();
            Nobles = new List<Noble>();
            ReservedCards = new List<Card>();
            CardsInPlay = new List<Card>();

            Bonuses = GetDiscount();
            Budget = Bonuses.MergeWith(Purse);
            VictoryPoints = GetVictoryPoints();
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

            Bonuses = GetDiscount();
            Budget = Bonuses.MergeWith(Purse);
            VictoryPoints = GetVictoryPoints();
        }

        public Player Clone(IPool withPurse = null,
                            IReadOnlyCollection<Card> withReservedCards = null,
                            IReadOnlyCollection<Card> withCardsInPlay = null,
                            IReadOnlyCollection<Noble> withNobles = null)
        {
            return new Player(Name, 
                              withPurse ?? Purse, 
                              withReservedCards ?? ReservedCards,
                              withCardsInPlay ?? CardsInPlay, 
                              withNobles ?? Nobles);
        }

        public Player CloneWithNoble(Noble noble)
        {
            if (noble is null)
            {
                throw new ArgumentNullException(nameof(noble));
            }
            var nobles = new List<Noble>(Nobles);

            nobles.Add(noble);

            return new Player(Name,
                              Purse,
                              ReservedCards,
                              CardsInPlay,
                              nobles);
        }

        public IPool Purse { get; private set; }
        public IReadOnlyCollection<Card> ReservedCards { get; private set; }
        public IReadOnlyCollection<Card> CardsInPlay { get; private set; }
        public IReadOnlyCollection<Noble> Nobles { get; private set; }
        public IPool Budget { get; private set; }
        public IPool Bonuses { get; private set; }
        public int VictoryPoints { get; private set; }

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
