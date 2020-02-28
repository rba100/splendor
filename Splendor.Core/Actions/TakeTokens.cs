
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.Domain;

namespace Splendor.Core.Actions
{
    public class TakeTokens : IAction
    {
        public IReadOnlyDictionary<TokenColour, int> TokensToTake { get; }
        public IReadOnlyDictionary<TokenColour, int> TokensToReturn { get; }

        public TakeTokens(IReadOnlyDictionary<TokenColour, int> tokensToTake,
                         IReadOnlyDictionary<TokenColour, int> tokensToReturn = null)
        {
            TokensToTake = tokensToTake ?? throw new ArgumentNullException(nameof(tokensToTake));
            TokensToReturn = tokensToReturn ?? Utility.CreateEmptyTokenPool();
        }

        public override string ToString()
        {
            var things = TokensToTake.Where(c => c.Value > 0).Select(kvp => (kvp.Value == 1 ? "" : $"{kvp.Value} ") + $"{kvp.Key}");
            return "Taking " + string.Join(", ", things);
        }

        public GameState Execute(GameState gameState)
        {
            // Enforce rules
            Validate(gameState);

            var nextAvailableTokens = gameState.TokensAvailable.CreateCopy();
            var nextPlayerTokens = gameState.CurrentPlayer.Purse.CreateCopy();

            // Perform transaction
            foreach (var colour in gameState.TokensAvailable.Keys.ToArray())
            {
                // Take coins
                nextAvailableTokens[colour] -= TokensToTake[colour];
                nextPlayerTokens[colour] += TokensToTake[colour];

                // Return coins
                nextAvailableTokens[colour] += TokensToReturn[colour];
                nextPlayerTokens[colour] -= TokensToReturn[colour];
            }

            var player = gameState.CurrentPlayer.Clone(withPurse: nextPlayerTokens);

            return gameState.CopyWith(coinsAvailable: nextAvailableTokens).CopyWithPlayer(player);
        }

        private void Validate(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

            if (TokensToTake.Values.Sum() > 3)
            {
                throw new RulesViolationException("You can only take up to three different tokens, or two of the same if there are four of that type available");
            }

            if (TokensToTake[TokenColour.Gold] > 0)
            {
                throw new RulesViolationException("You can't take gold tokens, except when reserving cards.");
            }

            if (TokensToTake.Any(kvp => kvp.Value == 2)) // If two taken
            {
                var doubleCoinColour = TokensToTake.Single(c => c.Value == 2).Key;

                // No other tokens can be taken
                if (TokensToTake.Count(kvp => kvp.Value == 0) != TokensToTake.Count - 1)
                {
                    throw new RulesViolationException("You can take two tokens of the same colour only if you take just those two tokens.");
                }
                if (gameState.TokensAvailable[doubleCoinColour] < 4)
                {
                    throw new RulesViolationException("You can only take two tokens if there are four or more available.");
                }
            }

            var totalCoinsAfterTransaction = player.Purse.Values.Sum() + TokensToTake.Values.Sum() - TokensToReturn.Values.Sum();
            if (totalCoinsAfterTransaction > 10) throw new RulesViolationException("A player cannot end a turn with more than 10 tokens.");

            foreach (var colour in TokensToTake.Keys)
            {
                if (gameState.TokensAvailable[colour] < TokensToTake[colour])
                    throw new RulesViolationException($"There aren't enough {colour} tokens to take.");
            }

            foreach (var colour in TokensToReturn.Keys)
            {
                if (player.Purse[colour] + TokensToTake[colour] < TokensToReturn[colour])
                    throw new RulesViolationException($"The player doesn't have enough {colour} tokens to give back.");
            }
        }
    }
}
