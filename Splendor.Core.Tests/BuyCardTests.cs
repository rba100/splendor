using NUnit.Framework;
using Splendor.Core.Actions;

namespace Splendor.Core.Tests
{
    [TestFixture]
    public class BuyCardTests
    {
        [Test]
        public void CanAffordCard_uses_gold()
        {
            // Costs 1 white, 2 blue.
            var card = new Card(1, 0, new Pool(0, 1, 2, 0, 0, 0), TokenColour.Black);

            var poolWithGold = new Pool(1, 1, 1, 0, 0, 0);

            Assert.True(BuyCard.CanAffordCard(poolWithGold, card));
        }

        [Test]
        public void CannotAffordCard_if_wrong_coins()
        {
            // Costs 1 white, 2 blue.
            var card = new Card(1, 0, new Pool(0, 1, 2, 0, 0, 0), TokenColour.Black);

            var poolWithGold = new Pool(0, 1, 1, 0, 0, 1);

            Assert.False(BuyCard.CanAffordCard(poolWithGold, card));
        }
    }
}