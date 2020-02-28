using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor
{
    public class Player
    {
        public string Name { get; }

        internal Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Purse = Utility.CreateEmptyTokenPool();
            Nobles = new List<Noble>();
            ReservedCards = new List<Card>();
            CardsInPlay = new List<Card>();
        }

        private Player(string name,
                       IReadOnlyDictionary<TokenColour, int> purse,
                       IReadOnlyCollection<Card> reservedCards, 
                       IReadOnlyCollection<Card> cardsInPlay,
                       IReadOnlyCollection<Noble> nobles) : this(name)
        {
            Purse = purse ?? throw new ArgumentNullException(nameof(purse));
            ReservedCards = reservedCards ?? throw new ArgumentNullException(nameof(reservedCards));
            CardsInPlay = cardsInPlay ?? throw new ArgumentNullException(nameof(cardsInPlay));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
        }

        public Player Clone(IReadOnlyDictionary<TokenColour, int> withPurse = null,
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

        public IReadOnlyDictionary<TokenColour, int> Purse { get; private set; }
        public IReadOnlyCollection<Card> ReservedCards { get; private set; }
        public IReadOnlyCollection<Card> CardsInPlay { get; private set; }
        public IReadOnlyCollection<Noble> Nobles { get; private set; }

        public IReadOnlyDictionary<TokenColour, int> GetDiscount()
        {
            return Enum.GetValues(typeof(TokenColour))
                       .OfType<TokenColour>()
                       .ToDictionary(col => col, col => CardsInPlay.Count(c => c.BonusGiven == col));
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
