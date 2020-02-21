using Splendor.Core.Domain;
using System;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class ReserveFaceDownCard : IAction
    {
        public int Tier { get; }
        CoinColour? ColourToReturnIfMaxCoins { get; }

        public ReserveFaceDownCard(int tier, CoinColour? colourToReturnIfMaxCoins = null)
        {
            Tier = tier;
            ColourToReturnIfMaxCoins = colourToReturnIfMaxCoins;
        }

        public override string ToString()
        {
            return $"Reserving a random card from tier {Tier}";
        }

        public void Execute(IGameEngine gameEngine)
        {
            var player = gameEngine.GameState.CurrentPlayer;
            var boardTier = gameEngine.GameState.Tiers.Single(t => t.Tier == Tier);
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

            if(gameEngine.GameState.CoinsAvailable[CoinColour.Gold] > 1)
            {
                if(player.Purse.Values.Sum() >= 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue 
                        ? ColourToReturnIfMaxCoins.Value 
                        : player.Purse.First(kvp => kvp.Value > 0).Key;

                    if(player.Purse[colourToReturn] < 1 && ColourToReturnIfMaxCoins != CoinColour.Gold)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    player.Purse[colourToReturn]--;
                    gameEngine.GameState.CoinsAvailable[colourToReturn]++;
                }

                gameEngine.GameState.CoinsAvailable[CoinColour.Gold]--;
                player.Purse[CoinColour.Gold]++;
            }

            gameEngine.CommitTurn();
        }
    }
}
