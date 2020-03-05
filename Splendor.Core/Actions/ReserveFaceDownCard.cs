
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.Domain;

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
            var playerPurse = player.Purse.CreateCopy();
            var nextTokensAvailable = gameState.TokensAvailable.CreateCopy();

            nextTiers.Remove(tier);
            try
            {
                var (nextTier, cardTaken) = tier.CloneAndTakeFaceDownCard();
                nextTiers.Add(nextTier);
                playerReserved.Add(cardTaken);
            }
            catch (InvalidOperationException ex)
            {
                throw new RulesViolationException("There are no face down cards remaining in tier " + Tier);
            }

            if (gameState.TokensAvailable[TokenColour.Gold] > 1)
            {
                if (player.Purse.Sum >= 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue
                        ? ColourToReturnIfMaxCoins.Value
                        : player.Purse.Colours().First(col => col != TokenColour.Gold);

                    if (player.Purse[colourToReturn] < 1 && ColourToReturnIfMaxCoins != TokenColour.Gold)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    playerPurse[colourToReturn]--;
                    nextTokensAvailable[colourToReturn]++;
                }

                nextTokensAvailable[TokenColour.Gold]--;
                playerPurse[TokenColour.Gold]++;
            }

            var nextPlayers = new List<Player>();
            foreach (var p in gameState.Players) if (p.Name == player.Name)
                    nextPlayers.Add(player.Clone(playerPurse, playerReserved, playerCardsInPlay));
                else nextPlayers.Add(p);

            return gameState.Clone(nextTokensAvailable, withTiers: nextTiers, withPlayers: nextPlayers);
        }
    }
}
