
using System.Linq;

using Splendor.Core.Actions;

using NUnit.Framework;

namespace Splendor.Core.Tests.Actions
{
    [TestFixture]
    public class ReserveFaceDownCardTests
    {
        private static GameState DefaultGame = new DefaultGameInitialiser(new DefaultCards()).Create(new[] { "a", "b", "c" });

        [Test]
        public void Reserve_gives_correct_card_and_gold()
        {
            var action = new ReserveFaceDownCard(1);

            var nextGameState = action.Execute(DefaultGame);

            Assert.AreEqual(1, nextGameState.CurrentPlayer.ReservedCards.Count());
            Assert.AreEqual(1, nextGameState.CurrentPlayer.Purse.Gold);
            Assert.AreEqual(4, nextGameState.Bank.Gold);
        }

        [Test]
        public void Reserve_gives_correct_card_and_no_gold_if_bank_has_no_gold()
        {
            var action = new ReserveFaceDownCard(1);

            var nextGameState = action.Execute(DefaultGame.Clone(withTokensAvailable: new Pool())); // Bank has no tokens

            Assert.AreEqual(1, nextGameState.CurrentPlayer.ReservedCards.Count());
            Assert.AreEqual(0, nextGameState.CurrentPlayer.Purse.Gold);
        }
    }
}
