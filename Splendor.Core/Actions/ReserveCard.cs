using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class ReserveCard : IAction
    {
        public Card Card { get; }
        TokenColour? ColourToReturnIfMaxCoins { get; }

        public ReserveCard(Card card, TokenColour? colourToReturnIfMaxCoins = null)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));

            ColourToReturnIfMaxCoins = colourToReturnIfMaxCoins;
        }

        public override string ToString()
        {
            return $"Reserving {Card}";
        }

        public GameState Execute(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

            if (player.ReservedCards.Count >= 3)
            {
                throw new RulesViolationException("You can't reserve more than three cards.");
            }

            var tier = gameState.Tiers.SingleOrDefault(t => t.ColumnSlots.Values.Contains(Card));

            if (tier == null)
            {
                throw new RulesViolationException("That card isn't on the board.");
            }

            var nextTiers = new List<BoardTier>(gameState.Tiers);
            var nextTier = gameState.Tiers.Single(t => t.Tier == Card.Tier);
            nextTiers.Remove(nextTier);
            nextTiers.Add(nextTier.Clone(withCardTaken: Card));
            var playerReserved = new List<Card>(player.ReservedCards);
            var playerPurse = player.Purse.CreateCopy();
            var nextTokensAvailable = gameState.Bank.CreateCopy();

            playerReserved.Add(Card);

            if (gameState.Bank[TokenColour.Gold] > 0)
            {
                nextTokensAvailable[TokenColour.Gold]--;
                playerPurse[TokenColour.Gold]++;

                if (player.Purse.Sum > 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue
                        ? ColourToReturnIfMaxCoins.Value
                        : player.Purse.Colours().First(col => col != TokenColour.Gold);
                    
                    if (player.Purse[colourToReturn] < 1)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    playerPurse[colourToReturn]--;
                    nextTokensAvailable[colourToReturn]++;
                }
            }

            var nextPlayer = player.Clone(playerPurse, playerReserved);
            return gameState.Clone(nextTokensAvailable, withTiers: nextTiers).CloneWithPlayerReplacedByName(nextPlayer);
        }
    }
}
