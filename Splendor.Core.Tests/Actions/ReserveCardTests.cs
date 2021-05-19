
using System.Linq;
using System.Collections.Generic;

using Splendor.Core.Actions;

using NUnit.Framework;

namespace Splendor.Core.Tests.Actions
{
    [TestFixture]
    public class ReserveCardTests
    {
        private static GameState DefaultGame = new DefaultGameInitialiser(new DefaultCards()).Create(new[] { "a", "b", "c" });

        [Test]
        public void Reserve_gives_correct_card_and_gold()
        {
            var card = DefaultGame.Tiers.First().ColumnSlots[0];
            var action = new ReserveCard(card);

            var nextGameState = action.Execute(DefaultGame);

            Assert.AreEqual(card, nextGameState.CurrentPlayer.ReservedCards.Single());
            Assert.AreEqual(1, nextGameState.CurrentPlayer.Purse.Gold);
            Assert.AreEqual(4, nextGameState.Bank.Gold);
        }

        [Test]
        public void Reserve_gives_correct_card_and_no_gold_if_bank_has_no_gold()
        {
            var card = DefaultGame.Tiers.First().ColumnSlots[0];
            var action = new ReserveCard(card);

            var nextGameState = action.Execute(DefaultGame with { Bank = new Pool() }); // Bank has no tokens

            Assert.AreEqual(card, nextGameState.CurrentPlayer.ReservedCards.Single());
            Assert.AreEqual(0, nextGameState.CurrentPlayer.Purse.Gold);
        }

        [Test]
        public void Cannot_reserve_a_card_not_in_play()
        {
            var (_, card) = DefaultGame.Tiers.First().CloneAndTakeFaceDownCard();
            var action = new ReserveCard(card);
            Assert.Throws<RulesViolationException>(() => action.Execute(DefaultGame));
        }

        [Test]
        public void Cannot_reserve_a_bought_card()
        {
            var (tier, card) = DefaultGame.Tiers.First().CloneAndTakeFaceDownCard();
            var player = DefaultGame.CurrentPlayer with { CardsInPlay = new[] { card } };
            var tiers = new List<BoardTier>(DefaultGame.Tiers);
            tiers.RemoveAll(t => t.Tier == tier.Tier);
            tiers.Add(tier);
            var gameState = DefaultGame.WithUpdatedPlayerByName(player) with { Tiers = tiers };

            var action = new ReserveCard(card);
            Assert.Throws<RulesViolationException>(() => action.Execute(gameState));
        }
    }
}
