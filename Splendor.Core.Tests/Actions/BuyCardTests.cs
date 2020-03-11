using NUnit.Framework;
using Splendor.Core.Actions;
using System.Linq;

namespace Splendor.Core.Tests.Actions
{
    [TestFixture]
    public class BuyCardTests
    {
        private static GameState DefaultGame = new DefaultGameInitialiser(new DefaultCards()).Create(new[] { "a", "b", "c" });

        [Test]
        public void CanAffordCard_uses_gold()
        {
            // Costs 1 white, 2 blue.
            var card = new Card(1, 0, new Pool(0, 1, 2, 0, 0, 0), TokenColour.Black);

            var budget = new Pool(1, 1, 1, 0, 0, 0);
            var player = new Player("").Clone(withPurse: budget);
            Assert.True(BuyCard.CanAffordCard(player, card));
        }

        [Test]
        public void CannotAffordCard_if_wrong_coins()
        {
            var card = new Card(1, 0, new Pool(0, 1, 2, 0, 0, 0), TokenColour.Black);
            var budget = new Pool(0, 1, 1, 0, 0, 1);
            var player = new Player("").Clone(withPurse: budget);
            Assert.False(BuyCard.CanAffordCard(player, card));
        }

        [Test]
        public void Default_payment_is_cost_when_no_gold_needed()
        {
            var card = new Card(1, 0, new Pool(0, 1, 0, 1, 0, 1), TokenColour.Black);
            var budget = new Pool(0, 2, 2, 2, 2, 2);
            var player = new Player("").Clone(withPurse: budget);
            IPool payment = BuyCard.CreateDefaultPaymentOrNull(player, card);

            Assert.That(PoolsAreExactlyEquivilent(card.Cost, payment));
        }

        [Test]
        public void Default_payment_is_null_when_cannot_afford()
        {
            var card = new Card(1, 0, new Pool(0, 1, 0, 1, 0, 0), TokenColour.Black);

            var budget = new Pool(1, 0, 0, 0, 1, 1);
            var player = new Player("").Clone(withPurse: budget);

            IPool payment = BuyCard.CreateDefaultPaymentOrNull(player, card);

            Assert.Null(payment);
        }

        [Test]
        public void BuyCard_can_use_bonus()
        {
            var card = new Card(1, 0, new Pool(0, 0, 2, 0, 2, 1), TokenColour.Black);
            var cardInPlay = new Card(1, 0, new Pool(), TokenColour.Green);
            var purse = new Pool(0, 1, 2, 0, 1, 1);

            var player = DefaultGame.CurrentPlayer
                .Clone(withPurse: purse, withCardsInPlay: new []{ cardInPlay });

            var tiers = DefaultGame.Tiers.ToList();
            tiers.RemoveAll(r => r.Tier == 1);
            tiers.Add(new BoardTier(1, new[] { card }, 1));

            var gameState = DefaultGame.CloneWithPlayerReplacedByName(player).Clone(withTiers: tiers);

            var payment = BuyCard.CreateDefaultPaymentOrNull(player, card);
            Assert.AreEqual(4, payment.Sum);
            var action = new BuyCard(card, payment);
            var nextState = action.Execute(gameState);
        }

        private static bool PoolsAreExactlyEquivilent(IPool a, IPool b)
        {
            return a.Gold == b.Gold
                && a.Black == b.Black
                && a.Blue == b.Blue
                && a.Red == b.Red
                && a.White == b.White
                && a.Green == b.Green;
        }
    }
}