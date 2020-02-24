using System.Collections.Generic;

namespace Splendor
{
    public sealed class BoardTier
    {
        internal BoardTier(int tier, IEnumerable<Card> cards, int columns)
        {
            Tier = tier;
            FaceDownCards = new Queue<Card>(cards);
            ColumnSlots = new Dictionary<int, Card>();

            for(var i = 0; i < columns; i++)
            {
                if(FaceDownCards.Count > 0) ColumnSlots[i] = FaceDownCards.Dequeue();
            }
        }

        public int Tier { get; private set; }
        public Queue<Card> FaceDownCards { get; private set; }
        public IDictionary<int,Card> ColumnSlots { get; private set; }
    }
}
