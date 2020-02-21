using System.Collections.Generic;

namespace Splendor
{
    public class BoardTier
    {
        public BoardTier(IEnumerable<Card> cards, int columns)
        {
            FaceDownCards = new Queue<Card>(cards);
            ColumnSlots = new Dictionary<int, Card>();

            for(var i = 0; i < columns; i++)
            {
                if(FaceDownCards.Count > 0) ColumnSlots[i] = FaceDownCards.Dequeue();
            }
        }

        public Queue<Card> FaceDownCards { get; private set; }
        public IDictionary<int,Card> ColumnSlots { get; private set; }
    }
}
