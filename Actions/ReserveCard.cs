using Splendor.Core.Domain;
using System;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class ReserveCard : IAction
    {
        public Card Card { get; }
        CoinColour? ColourToReturnIfMaxCoins { get; }

        public ReserveCard(Card card, CoinColour? colourToReturnIfMaxCoins = null)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));

            ColourToReturnIfMaxCoins = colourToReturnIfMaxCoins;
        }

        public override string ToString()
        {
            return $"Reserved {Card}";
        }

        public void Execute(IGame game)
        {
            var player = game.State.CurrentPlayer;

            if (player.ReservedCards.Count > 3)
            {
                throw new RulesViolationException("You can't reserve more than three cards.");
            }

            var tier = game.State.Tiers.SingleOrDefault(t => t.ColumnSlots.Values.Contains(Card));

            if (tier == null)
            {
                throw new RulesViolationException("That card isn't on the board.");
            }

            var index = tier.ColumnSlots.Single(s => s.Value == Card).Key;

            tier.ColumnSlots[index] = tier.FaceDownCards.Count > 0 ? tier.FaceDownCards.Dequeue() : null;
            player.ReservedCards.Add(Card);

            if (game.State.CoinsAvailable[CoinColour.Gold] > 1)
            {
                if (player.Purse.Values.Sum() >= 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue
                        ? ColourToReturnIfMaxCoins.Value
                        : player.Purse.First(kvp => kvp.Value > 0).Key;

                    if (player.Purse[colourToReturn] < 1 && ColourToReturnIfMaxCoins != CoinColour.Gold)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    player.Purse[colourToReturn]--;
                    game.State.CoinsAvailable[colourToReturn]++;
                }

                game.State.CoinsAvailable[CoinColour.Gold]--;
                player.Purse[CoinColour.Gold]++;
            }

            game.CommitTurn();
        }
    }
}
