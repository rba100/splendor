using Splendor.Core;
using Splendor.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public sealed class BoardTier
    {
        private Dictionary<int, Card> _columnSlots;
        private Queue<Card> _faceDownCards;

        internal BoardTier(int tier, IEnumerable<Card> cards, int columns)
        {
            Tier = tier;
            _faceDownCards = new Queue<Card>(cards);
            _columnSlots = new Dictionary<int, Card>();

            for (var i = 0; i < columns; i++)
            {
                if (_faceDownCards.Count > 0) _columnSlots[i] = _faceDownCards.Dequeue();
            }
        }

        private BoardTier(int tier, Dictionary<int, Card> columnSlots, Queue<Card> faceDownCards)
        {
            _faceDownCards = faceDownCards ?? throw new ArgumentNullException(nameof(faceDownCards));
            _columnSlots = columnSlots;
            Tier = tier;
        }

        public BoardTier Clone(Card withCardTaken = null)
        {
            Dictionary<int, Card> columnSlots = _columnSlots.CreateCopy();
            var i = columnSlots.Single(kvp => kvp.Value == withCardTaken).Key;
            var q = new Queue<Card>(_faceDownCards);
            columnSlots[i] = q.Count > 0 ? q.Dequeue() : null;
            return new BoardTier(Tier, columnSlots, q);
        }

        public (BoardTier, Card) CloneAndTakeFaceDownCard()
        {
            if (!HasFaceDownCardsRemaining) throw new InvalidOperationException("There are no face down cards remaining in tier " + Tier);
            var q = new Queue<Card>(_faceDownCards);
            var card = q.Dequeue();
            return (new BoardTier(Tier, _columnSlots.CreateCopy(), q), card);
        }

        public int Tier { get; private set; }
        public bool HasFaceDownCardsRemaining => _faceDownCards.Count > 0;
        public IReadOnlyDictionary<int, Card> ColumnSlots { get => _columnSlots; }
    }
}
