using Splendor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class DefaultCards : IGameDataSource
    {
        public IReadOnlyCollection<Card> AllCards()
        {
            return Cards().ToList();
        }

        public IReadOnlyCollection<Noble> AllNobles()
        {
            return Nobles().ToList();
        }

        private IEnumerable<Noble> Nobles()
        {
            yield return CreateFromStats(3, 3, 3, 3, 0, 0);
            yield return CreateFromStats(3, 0, 0, 4, 0, 4);
            yield return CreateFromStats(3, 0, 4, 0, 4, 0);
            yield return CreateFromStats(3, 4, 4, 0, 0, 0);
            yield return CreateFromStats(3, 0, 0, 0, 4, 4);
            yield return CreateFromStats(3, 0, 0, 3, 3, 3);
            yield return CreateFromStats(3, 0, 3, 0, 3, 3);
            yield return CreateFromStats(3, 4, 0, 4, 0, 0);
            yield return CreateFromStats(3, 3, 3, 0, 3, 0);
            yield return CreateFromStats(3, 3, 0, 3, 0, 3);
        }

        private IEnumerable<Card> Cards()
        {
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 1, 1, 1, 1);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 0, 1, 0, 2);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 2, 0, 0, 2);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 1, 0, 3, 0, 1);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 0, 0, 0, 3);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 1, 1, 2, 1);
            yield return CreateFromStats(TokenColour.Black, 1, 0, 0, 2, 1, 2, 0);
            yield return CreateFromStats(TokenColour.Black, 1, 1, 0, 0, 0, 4, 0);
            yield return CreateFromStats(TokenColour.Black, 2, 1, 0, 3, 0, 2, 2);
            yield return CreateFromStats(TokenColour.Black, 2, 1, 2, 3, 0, 0, 3);
            yield return CreateFromStats(TokenColour.Black, 2, 2, 0, 0, 2, 1, 4);
            yield return CreateFromStats(TokenColour.Black, 2, 2, 0, 5, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Black, 2, 2, 0, 0, 3, 0, 5);
            yield return CreateFromStats(TokenColour.Black, 2, 3, 6, 0, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Black, 3, 3, 0, 3, 3, 3, 5);
            yield return CreateFromStats(TokenColour.Black, 3, 4, 0, 0, 7, 0, 0);
            yield return CreateFromStats(TokenColour.Black, 3, 4, 3, 0, 6, 0, 3);
            yield return CreateFromStats(TokenColour.Black, 3, 5, 3, 0, 7, 0, 0);

            yield return CreateFromStats(TokenColour.Blue, 1, 0, 2, 1, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 1, 1, 2, 0, 1);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 1, 1, 1, 0, 1);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 0, 0, 1, 1, 3);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 3, 0, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 0, 1, 2, 0, 2);
            yield return CreateFromStats(TokenColour.Blue, 1, 0, 2, 0, 0, 0, 2);
            yield return CreateFromStats(TokenColour.Blue, 1, 1, 0, 0, 4, 0, 0);
            yield return CreateFromStats(TokenColour.Blue, 2, 1, 0, 0, 3, 2, 2);
            yield return CreateFromStats(TokenColour.Blue, 2, 1, 3, 0, 0, 2, 3);
            yield return CreateFromStats(TokenColour.Blue, 2, 2, 0, 5, 0, 3, 0);
            yield return CreateFromStats(TokenColour.Blue, 2, 2, 0, 0, 0, 5, 0);
            yield return CreateFromStats(TokenColour.Blue, 2, 2, 4, 2, 1, 0, 0);
            yield return CreateFromStats(TokenColour.Blue, 2, 3, 0, 0, 0, 6, 0);
            yield return CreateFromStats(TokenColour.Blue, 3, 3, 5, 3, 3, 0, 3);
            yield return CreateFromStats(TokenColour.Blue, 3, 4, 0, 7, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Blue, 3, 4, 3, 6, 0, 3, 0);
            yield return CreateFromStats(TokenColour.Blue, 3, 5, 0, 7, 0, 3, 0);

            yield return CreateFromStats(TokenColour.Green, 1, 0, 0, 2, 0, 1, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 0, 0, 2, 2, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 0, 1, 0, 3, 1);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 1, 1, 1, 1, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 2, 1, 1, 1, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 2, 0, 2, 1, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 0, 0, 0, 3, 0, 0);
            yield return CreateFromStats(TokenColour.Green, 1, 1, 4, 0, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Green, 2, 1, 0, 3, 3, 0, 2);
            yield return CreateFromStats(TokenColour.Green, 2, 1, 2, 2, 0, 3, 0);
            yield return CreateFromStats(TokenColour.Green, 2, 2, 1, 4, 0, 2, 0);
            yield return CreateFromStats(TokenColour.Green, 2, 2, 0, 0, 0, 0, 5);
            yield return CreateFromStats(TokenColour.Green, 2, 2, 0, 0, 0, 5, 3);
            yield return CreateFromStats(TokenColour.Green, 2, 3, 0, 0, 0, 0, 6);
            yield return CreateFromStats(TokenColour.Green, 3, 3, 3, 5, 3, 3, 0);
            yield return CreateFromStats(TokenColour.Green, 3, 4, 0, 3, 0, 6, 3);
            yield return CreateFromStats(TokenColour.Green, 3, 4, 0, 0, 0, 7, 0);
            yield return CreateFromStats(TokenColour.Green, 3, 5, 0, 0, 0, 7, 3);

            yield return CreateFromStats(TokenColour.Red, 1, 0, 0, 3, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 3, 1, 1, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 0, 0, 0, 2, 1);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 2, 2, 0, 0, 1);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 1, 2, 0, 1, 1);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 1, 1, 0, 1, 1);
            yield return CreateFromStats(TokenColour.Red, 1, 0, 0, 2, 2, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 1, 1, 0, 4, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 2, 1, 3, 0, 2, 3, 0);
            yield return CreateFromStats(TokenColour.Red, 2, 1, 3, 2, 2, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 2, 2, 0, 1, 0, 4, 2);
            yield return CreateFromStats(TokenColour.Red, 2, 2, 5, 3, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 2, 2, 5, 0, 0, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 2, 3, 0, 0, 6, 0, 0);
            yield return CreateFromStats(TokenColour.Red, 3, 3, 3, 3, 0, 5, 3);
            yield return CreateFromStats(TokenColour.Red, 3, 4, 0, 0, 0, 0, 7);
            yield return CreateFromStats(TokenColour.Red, 3, 4, 0, 0, 3, 3, 6);
            yield return CreateFromStats(TokenColour.Red, 3, 5, 0, 0, 3, 0, 7);

            yield return CreateFromStats(TokenColour.White, 1, 0, 1, 0, 0, 2, 2);
            yield return CreateFromStats(TokenColour.White, 1, 0, 1, 0, 2, 0, 0);
            yield return CreateFromStats(TokenColour.White, 1, 0, 1, 0, 1, 1, 1);
            yield return CreateFromStats(TokenColour.White, 1, 0, 0, 0, 0, 3, 0);
            yield return CreateFromStats(TokenColour.White, 1, 0, 0, 0, 0, 2, 2);
            yield return CreateFromStats(TokenColour.White, 1, 0, 1, 0, 1, 1, 2);
            yield return CreateFromStats(TokenColour.White, 1, 0, 1, 3, 0, 1, 0);
            yield return CreateFromStats(TokenColour.White, 1, 1, 0, 0, 0, 0, 4);
            yield return CreateFromStats(TokenColour.White, 2, 1, 2, 0, 2, 0, 3);
            yield return CreateFromStats(TokenColour.White, 2, 1, 0, 2, 3, 3, 0);
            yield return CreateFromStats(TokenColour.White, 2, 2, 2, 0, 4, 0, 1);
            yield return CreateFromStats(TokenColour.White, 2, 2, 0, 0, 5, 0, 0);
            yield return CreateFromStats(TokenColour.White, 2, 2, 3, 0, 5, 0, 0);
            yield return CreateFromStats(TokenColour.White, 2, 3, 0, 6, 0, 0, 0);
            yield return CreateFromStats(TokenColour.White, 3, 3, 3, 0, 5, 3, 3);
            yield return CreateFromStats(TokenColour.White, 3, 4, 7, 0, 0, 0, 0);
            yield return CreateFromStats(TokenColour.White, 3, 4, 6, 3, 3, 0, 0);
            yield return CreateFromStats(TokenColour.White, 3, 5, 7, 3, 0, 0, 0);
        }

        public Noble CreateFromStats(int victoryPoints, int black, int white, int red, int blue, int green)
        {
            var cost = new Pool(0, white, blue, red, green, black);
            return new Noble(victoryPoints, cost);
        }

        private Card CreateFromStats(TokenColour colour, int tier, int points, int black, int white, int red, int blue, int green)
        {
            var cost = new Pool(0, white, blue, red, green, black);
            return new Card(tier, points, cost, colour);
        }
    }
}
