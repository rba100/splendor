using Splendor.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class TakeCoins : IAction
    {
        public IReadOnlyDictionary<CoinColour, int> CoinsTaken { get; }
        public IReadOnlyDictionary<CoinColour, int> CoinsReturned { get; }

        public TakeCoins(IReadOnlyDictionary<CoinColour, int> coinsTaken, 
                         IReadOnlyDictionary<CoinColour, int> coinsReturned = null)
        {
            CoinsTaken = coinsTaken ?? throw new ArgumentNullException(nameof(coinsTaken));
            CoinsReturned = coinsReturned ?? Utility.CoinQuantity();           
        }

        public void Execute(GameEngine gameEngine)
        {
            // Enforce rules
            Validate(gameEngine);

            // Perform transaction
            foreach (var colour in gameEngine.GameState.CoinsAvailable.Keys)
            {
                // Take coins
                gameEngine.GameState.CoinsAvailable[colour] -= CoinsTaken[colour];
                gameEngine.GameState.CurrentPlayer.Purse[colour] += CoinsTaken[colour];

                // Return coins
                gameEngine.GameState.CoinsAvailable[colour] += CoinsReturned[colour];
                gameEngine.GameState.CurrentPlayer.Purse[colour] -= CoinsReturned[colour];
            }

            // End turn
            gameEngine.CommitTurn();
        }

        private void Validate(IGameEngine gameEngine)
        {
            var player = gameEngine.GameState.CurrentPlayer;

            if (CoinsTaken.Values.Sum() > 3)
            {
                throw new RulesViolationException("You can only take up to three different coins, or two of the same if there are four of that type available");
            }

            if (CoinsTaken[CoinColour.Gold] > 0)
            {
                throw new RulesViolationException("You can't take gold coins, except when reserving cards.");
            }

            if (CoinsTaken.Any(kvp => kvp.Value == 2)) // If two taken
            {
                var doubleCoinColour = CoinsTaken.Single(c => c.Value == 2).Key;

                // No other coins can be taken
                if (CoinsTaken.Count(kvp => kvp.Value == 0) != CoinsTaken.Count - 1)
                {
                    throw new RulesViolationException("You can only take two coins of the same colour if you take only those two coins.");
                }
                if (gameEngine.GameState.CoinsAvailable[doubleCoinColour] < 4)
                {
                    throw new RulesViolationException("You can only take two coins if there are four or more available.");
                }

            }

            var totalCoinsAfterTransaction = player.Purse.Values.Sum() + CoinsTaken.Values.Sum() - CoinsReturned.Values.Sum();
            if (totalCoinsAfterTransaction > 10) throw new RulesViolationException("A player cannot end a turn with more than 10 coins.");

            foreach (var colour in CoinsTaken.Keys)
            {
                if (gameEngine.GameState.CoinsAvailable[colour] < CoinsTaken[colour])
                    throw new RulesViolationException($"There aren't enough {colour} coins to take.");
            }

            foreach (var colour in CoinsReturned.Keys)
            {
                if (player.Purse[colour] + CoinsTaken[colour] < CoinsReturned[colour])
                    throw new RulesViolationException($"The player doesn't have enough {colour} coins to give back.");
            }
        }
    }
}
