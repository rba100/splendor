
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
            var boardTier = gameState.Tiers.Single(t => t.Tier == Tier);
            if (boardTier.FaceDownCards.Count == 0)
            {
                throw new RulesViolationException("There aren't any cards in that tier left to reserve.");
            }

            if(player.ReservedCards.Count > 3)
            {
                throw new RulesViolationException("You can't reserve more than three cards.");
            }

            var cardTaken = boardTier.FaceDownCards.Dequeue();
            player.ReservedCards.Add(cardTaken);

            if(gameState.TokensAvailable[TokenColour.Gold] > 1)
            {
                if(player.Purse.Values.Sum() >= 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue 
                        ? ColourToReturnIfMaxCoins.Value 
                        : player.Purse.First(kvp => kvp.Key != TokenColour.Gold && kvp.Value > 0).Key;

                    if (player.Purse[colourToReturn] < 1 && ColourToReturnIfMaxCoins != TokenColour.Gold)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    player.Purse[colourToReturn]--;
                    gameState.TokensAvailable[colourToReturn]++;
                }

                gameState.TokensAvailable[TokenColour.Gold]--;
                player.Purse[TokenColour.Gold]++;
            }

            return gameState;
        }
    }
}
