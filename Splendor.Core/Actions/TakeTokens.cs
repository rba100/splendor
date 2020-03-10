
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    public class TakeTokens : IAction
    {
        public IPool TokensToTake { get; }
        public IPool TokensToReturn { get; }

        public TakeTokens(IPool tokensToTake,
                          IPool tokensToReturn = null)
        {
            TokensToTake = tokensToTake ?? throw new ArgumentNullException(nameof(tokensToTake));
            TokensToReturn = tokensToReturn ?? new Pool();
        }

        public override string ToString()
        {
            var things = TokensToTake.Colours().Select(col => (TokensToTake[col] == 1 ? "" : $"{TokensToTake[col]} ") + $"{col}");
            var thingsBack = TokensToReturn.Colours().Select(col => (TokensToReturn[col] == 1 ? "" : $"{TokensToReturn[col]} ") + $"{col}");
            var returning = TokensToReturn.Sum == 0 ? "" : $", returning {string.Join(", ", thingsBack)}";
            return $"Taking {string.Join(", ", things)}{returning}";
        }

        public GameState Execute(GameState gameState)
        {
            // Enforce rules
            Validate(gameState);

            var nextAvailableTokens = gameState.Bank.CreateCopy();
            var nextPlayerTokens = gameState.CurrentPlayer.Purse.CreateCopy();

            // Perform transaction
            foreach (var colour in TokensToTake.Colours().Union(TokensToReturn.Colours()).ToArray())
            {
                // Take coins
                nextAvailableTokens[colour] -= TokensToTake[colour];
                nextPlayerTokens[colour] += TokensToTake[colour];

                // Return coins
                nextAvailableTokens[colour] += TokensToReturn[colour];
                nextPlayerTokens[colour] -= TokensToReturn[colour];
            }

            var player = gameState.CurrentPlayer.Clone(withPurse: nextPlayerTokens);

            return gameState.Clone(withTokensAvailable: nextAvailableTokens).CloneWithPlayerReplacedByName(player);
        }

        private void Validate(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

            if (TokensToTake.Sum > 3)
            {
                throw new RulesViolationException("You can only take up to three different tokens, or two of the same if there are four of that type available.");
            }
            else if (TokensToTake.Colours().Any(col => TokensToTake[col] > 2))
            {
                throw new RulesViolationException("You can only take up to two tokens of any one colour.");
            }

            if (TokensToTake[TokenColour.Gold] > 0)
            {
                throw new RulesViolationException("You can't take gold tokens, except when reserving cards.");
            }

            if (TokensToTake.Colours().Any(col => TokensToTake[col] == 2)) // If two taken
            {
                if (TokensToTake.Colours().Count() > 1)
                {
                    throw new RulesViolationException("You can take two tokens of the same colour only if you take just those two tokens.");
                }
                
                var doubleCoinColour = TokensToTake.Colours().Single();

                if (gameState.Bank[doubleCoinColour] < 4)
                {
                    throw new RulesViolationException("You can only take two tokens if there are four or more available.");
                }
            }

            var totalCoinsAfterTransaction = player.Purse.Sum + TokensToTake.Sum - TokensToReturn.Sum;
            if (totalCoinsAfterTransaction > 10) throw new RulesViolationException("A player cannot end a turn with more than 10 tokens.");

            foreach (var colour in TokensToTake.Colours())
            {
                if (gameState.Bank[colour] < TokensToTake[colour])
                    throw new RulesViolationException($"There aren't enough {colour} tokens to take.");
            }

            foreach (var colour in TokensToReturn.Colours())
            {
                if (player.Purse[colour] + TokensToTake[colour] < TokensToReturn[colour])
                    throw new RulesViolationException($"The player doesn't have enough {colour} tokens to give back.");
            }
        }
    }
}
