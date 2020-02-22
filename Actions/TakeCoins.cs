
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.Domain;

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
            CoinsReturned = coinsReturned ?? Utility.CreateEmptyCoinQuantity();
        }

        public override string ToString()
        {
            var things = CoinsTaken.Where(c => c.Value > 0).Select(kvp => (kvp.Value == 1 ? "" : $"{kvp.Value} ") + $"{kvp.Key}");
            return "Taking " + string.Join(", ", things);
        }

        public GameState Execute(GameState gameState)
        {
            // Enforce rules
            Validate(gameState);

            // Perform transaction
            foreach (var colour in gameState.CoinsAvailable.Keys.ToArray())
            {
                // Take coins
                gameState.CoinsAvailable[colour] -= CoinsTaken[colour];
                gameState.CurrentPlayer.Purse[colour] += CoinsTaken[colour];

                // Return coins
                gameState.CoinsAvailable[colour] += CoinsReturned[colour];
                gameState.CurrentPlayer.Purse[colour] -= CoinsReturned[colour];
            }

            return gameState;
        }

        private void Validate(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

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
                    throw new RulesViolationException("You can take two coins of the same colour only if you take just those two coins.");
                }
                if (gameState.CoinsAvailable[doubleCoinColour] < 4)
                {
                    throw new RulesViolationException("You can only take two coins if there are four or more available.");
                }
            }

            var totalCoinsAfterTransaction = player.Purse.Values.Sum() + CoinsTaken.Values.Sum() - CoinsReturned.Values.Sum();
            if (totalCoinsAfterTransaction > 10) throw new RulesViolationException("A player cannot end a turn with more than 10 coins.");

            foreach (var colour in CoinsTaken.Keys)
            {
                if (gameState.CoinsAvailable[colour] < CoinsTaken[colour])
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
