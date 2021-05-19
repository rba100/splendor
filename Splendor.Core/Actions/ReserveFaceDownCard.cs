
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class ReserveFaceDownCard : IAction
    {
        public int Tier { get; }
        TokenColour? ColourToReturnIfMaxCoins { get; }

        public ReserveFaceDownCard(int tier, TokenColour? colourToReturnIfMaxCoins = null)
        {
            Tier = tier;
            ColourToReturnIfMaxCoins = colourToReturnIfMaxCoins;
        }

        public override string ToString()
        {
            return $"Reserving a random card from tier {Tier}";
        }

        public GameState Execute(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

            if (player.ReservedCards.Count >= 3)
            {
                throw new RulesViolationException("You can't reserve more than three cards.");
            }

            var nextTiers = new List<BoardTier>(gameState.Tiers);
            var tier = gameState.Tiers.Single(t => t.Tier == Tier);
            var playerCardsInPlay = new List<Card>(player.CardsInPlay);
            var playerReserved = new List<Card>(player.ReservedCards);
            var nextPlayerPurse = player.Purse.CreateCopy();
            var nextTokensAvailable = gameState.Bank.CreateCopy();

            nextTiers.Remove(tier);
            try
            {
                var (nextTier, cardTaken) = tier.CloneAndTakeFaceDownCard();
                nextTiers.Add(nextTier);
                playerReserved.Add(cardTaken);
            }
            catch (InvalidOperationException)
            {
                throw new RulesViolationException("There are no face down cards remaining in tier " + Tier);
            }

            if (gameState.Bank[TokenColour.Gold] > 0)
            {
                nextTokensAvailable[TokenColour.Gold]--;
                nextPlayerPurse[TokenColour.Gold]++;

                if (nextPlayerPurse.Sum > 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue
                        ? ColourToReturnIfMaxCoins.Value
                        : player.Purse.Colours().First(col => col != TokenColour.Gold);

                    if (nextPlayerPurse[colourToReturn] < 1)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    nextPlayerPurse[colourToReturn]--;
                    nextTokensAvailable[colourToReturn]++;
                }
            }

            var nextPlayers = new List<Player>();
            foreach (var p in gameState.Players) if (p.Name == player.Name)
                    nextPlayers.Add(player with { Purse = nextPlayerPurse, ReservedCards = playerReserved, CardsInPlay = playerCardsInPlay });
                else nextPlayers.Add(p);

            return gameState with { Bank = nextTokensAvailable, Tiers = nextTiers, Players = nextPlayers };
        }
    }
}
