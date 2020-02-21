
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core.Domain;

namespace Splendor.Core.Actions
{
    /// <summary>
    /// Accepts either a reserved card or one from the face-up cards.
    /// </summary>
    public class BuyCard : IAction
    {
        public Player Player { get; }
        public Card Card { get; }
        public IReadOnlyDictionary<CoinColour, int> Payment { get; }

        public BuyCard(Player player, Card card, IReadOnlyDictionary<CoinColour, int> optionalSpecificPayment = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Payment = optionalSpecificPayment;
        }

        public void Execute(GameEngine gameEngine)
        {
            // Validate 
            ValidatePaymentIfNotNull();
            VerifyCardIsInHandOrBoard(gameEngine);
            var tier = gameEngine.GameState.Tiers.SingleOrDefault(t => t.ColumnSlots.Values.Contains(Card));               
            var payment = Payment ?? CreateDefaultPayment();

            // Perform transaction - card
            if (tier != null)
            {
                var index = tier.ColumnSlots.Single(s => s.Value == Card).Key;
                tier.ColumnSlots[index] = tier.FaceDownCards.Count > 0 ? tier.FaceDownCards.Dequeue() : null;
            }
            else
            {
                Player.ReservedCards.Remove(Card);
            }
            Player.CardsInPlay.Add(Card);

            // Perform transaction - payment
            foreach(var colour in payment.Keys)
            {
                Player.Purse[colour] -= payment[colour];
            }

            gameEngine.CommitTurn();
        }

        private void VerifyCardIsInHandOrBoard(IGameEngine gameEngine)
        {
            if(Player.ReservedCards.Contains(Card)) return;
            var tier = gameEngine.GameState.Tiers.SingleOrDefault(t => t.ColumnSlots.Values.Contains(Card));
            if (tier == null)
            {
                throw new RulesViolationException("That card isn't on the board or in hand.");
            }
        }

        private Dictionary<CoinColour, int> CreateDefaultPayment()
        {
            var payment = Utility.CreateEmptyTransaction();

            var available = Player.Purse.CreateCopy();
            var discount = Player.GetDiscount();
            var costRemaining = Card.Cost.CreateCopy();

            foreach (var colour in discount.Keys)
            {
                costRemaining[colour] = Math.Min(costRemaining[colour] - discount[colour], 0);
            }

            foreach (var colour in costRemaining.Keys)
            {
                if (costRemaining[colour] < 1) continue;
                if (costRemaining[colour] > available[colour] + available[CoinColour.Gold])
                {
                    throw new RulesViolationException("You can't afford this card.");
                }

                if (costRemaining[colour] < available[colour])
                {
                    payment[colour] = costRemaining[colour];
                }
                else
                {
                    var goldNeeded = costRemaining[colour] - available[colour];
                    payment[colour] = available[colour];

                    payment[CoinColour.Gold] += goldNeeded;
                    available[CoinColour.Gold] -= goldNeeded;
                }
            }

            return payment;
        }

        private void ValidatePaymentIfNotNull()
        {
            if (Payment == null) return;

            var available = Player.Purse.CreateCopy();
            var costRemaining = Card.Cost.CreateCopy();
            var discount = Player.GetDiscount();

            foreach (var colour in discount.Keys)
            {
                costRemaining[colour] = Math.Min(costRemaining[colour] - discount[colour], 0);
            }

            foreach (var colour in costRemaining.Keys)
            {
                if (costRemaining[colour] < 1) continue;
                if (costRemaining[colour] > available[colour] + available[CoinColour.Gold])
                {
                    throw new RulesViolationException("You can't afford this card with the supplied payment.");
                }

                if (costRemaining[colour] > available[colour])
                {
                    available[CoinColour.Gold] -= costRemaining[colour] - available[colour];
                }
            }
        }
    }
}
