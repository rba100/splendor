
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    /// <summary>
    /// Accepts either a reserved card or one from the face-up cards.
    /// </summary>
    public class BuyCard : IAction
    {
        public Card Card { get; }
        public IPool Payment { get; }

        public BuyCard(Card card, IPool payment)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Payment = payment ?? throw new ArgumentNullException(nameof(payment));
        }

        public override string ToString()
        {
            var costs = Payment?.Colours().Select(col => $"{Payment[col]} {col}").ToList();
            if (costs?.Count() == 0) return $"Buying [{Card}] for free.";
            return costs == null ? $"Buying {Card}" : $"Buying [{Card}] with {string.Join(", ", costs)}";
        }

        public GameState Execute(GameState gameState)
        {
            var player = gameState.CurrentPlayer;

            // Validate 
            if(!SuppliedPaymentIsValid(gameState)) throw new RulesViolationException("You can't afford this card with the supplied payment."); ;
            if(!VerifyCardIsInHandOrBoard(player, gameState, Card)) throw new RulesViolationException("That card isn't on the board or in hand.");

            var isFromHand = player.ReservedCards.Contains(Card);

            var nextTiers = new List<BoardTier>(gameState.Tiers);
            var nextTier = gameState.Tiers.Single(t => t.Tier == Card.Tier);
            var playerCardsInPlay = new List<Card>(player.CardsInPlay);
            var playerReserved = new List<Card>(player.ReservedCards);
            var playerPurse = player.Purse.CreateCopy();
            var nextTokensAvailable = gameState.Bank.CreateCopy();

            if (isFromHand)
            {
                playerReserved.Remove(Card);
            }
            else
            {
                nextTiers.Remove(nextTier);
                nextTiers.Add(nextTier.Clone(withCardTaken: Card));
            }

            playerCardsInPlay.Add(Card);

            //  - Payment
            foreach(var colour in Payment.Colours())
            {
                playerPurse[colour] -= Payment[colour];
                nextTokensAvailable[colour] += Payment[colour];
            }

            var nextPlayer = player.Clone(playerPurse, playerReserved, playerCardsInPlay);

            return gameState.CloneWithPlayerReplacedByName(nextPlayer).Clone(nextTokensAvailable, withTiers: nextTiers);
        }

        public static bool CanAffordCard(Player player, Card card)
        {
            return CreateDefaultPaymentOrNull(player, card) != null;
        }

        public static IPool CreateDefaultPaymentOrNull(Player player, Card card)
        {
            var payment = new Pool();
            var costRemaining = player.Bonuses.DeficitFor(card.Cost);
            var goldRemaining = player.Purse.Gold;

            foreach (var colour in costRemaining.Colours())
            {
                if (costRemaining[colour] < 1) continue;
                if (costRemaining[colour] > player.Purse[colour] + goldRemaining)
                {
                    return null;
                }

                if (costRemaining[colour] < player.Purse[colour])
                {
                    payment[colour] = costRemaining[colour];
                }
                else
                {
                    var goldNeeded = costRemaining[colour] - player.Purse[colour];
                    payment[colour] = player.Purse[colour];

                    payment[TokenColour.Gold] += goldNeeded;
                    goldRemaining -= goldNeeded;
                }
            }

            return payment;
        }

        private static bool VerifyCardIsInHandOrBoard(Player player, GameState gameState, Card card)
        {
            if (player.ReservedCards.Contains(card)) return true;
            var tier = gameState.Tiers.SingleOrDefault(t => t.ColumnSlots.Values.Contains(card));
            return tier != null;
        }

        private bool SuppliedPaymentIsValid(GameState gameState)
        {
            var available = gameState.CurrentPlayer.Purse.CreateCopy();
            var costRemaining = Card.Cost.CreateCopy();

            foreach (var colour in gameState.CurrentPlayer.Bonuses.Colours())
            {
                costRemaining[colour] = Math.Max(0, costRemaining[colour] - gameState.CurrentPlayer.Bonuses[colour]);
            }

            foreach (var colour in costRemaining.Colours())
            {
                if (costRemaining[colour] < 1) continue;
                if (costRemaining[colour] > available[colour] + available[TokenColour.Gold])
                {
                    return false;                    
                }

                if (costRemaining[colour] > available[colour])
                {
                    available[TokenColour.Gold] -= costRemaining[colour] - available[colour];
                }
            }
            return true;
        }
    }
}
