using Splendor.Core.Domain;
using System;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class ReserveFaceDownCard : IAction
    {
        public Player Player { get; }
        public int Tier { get; }
        CoinColour? ColourToReturnIfMaxCoins { get; }

        public ReserveFaceDownCard(Player player, int tier, CoinColour? colourToReturnIfMaxCoins = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Tier = tier;
            ColourToReturnIfMaxCoins = colourToReturnIfMaxCoins;
        }

        public void Execute(GameEngine gameEngine)
        {
            if(gameEngine.GameState.Tiers[Tier].FaceDownCards.Count == 0)
            {
                throw new RulesViolationException("There aren't any cards in that tier left to reserve.");
            }

            if(Player.ReservedCards.Count > 3)
            {
                throw new RulesViolationException("You can't reserve more than three cards.");
            }

            var cardTaken = gameEngine.GameState.Tiers[Tier].FaceDownCards.Dequeue();
            Player.ReservedCards.Add(cardTaken);

            if(gameEngine.GameState.CoinsAvailable[CoinColour.Gold] > 1)
            {
                if(Player.Purse.Values.Sum() >= 10)
                {
                    var colourToReturn = ColourToReturnIfMaxCoins.HasValue 
                        ? ColourToReturnIfMaxCoins.Value 
                        : Player.Purse.First(kvp => kvp.Value > 0).Key;

                    if(Player.Purse[colourToReturn] < 1 && ColourToReturnIfMaxCoins != CoinColour.Gold)
                    {
                        throw new RulesViolationException("You can't give back a coin you don't have.");
                    }

                    Player.Purse[colourToReturn]--;
                    gameEngine.GameState.CoinsAvailable[colourToReturn]++;
                }

                gameEngine.GameState.CoinsAvailable[CoinColour.Gold]--;
                Player.Purse[CoinColour.Gold]++;
            }

            gameEngine.CommitTurn();
        }
    }
}
