using Splendor.Core.Actions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.AI
{
    public class StupidSplendorAi : ISpendorAi
    {
        public string Name { get; private set; }

        public StupidSplendorAi(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IAction ChooseAction(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var allFaceUpCards = gameState.Tiers
                .SelectMany(t => t.ColumnSlots)                                                
                .Select(s => s.Value)                                                
                .Where(card => card != null) // If a stack runs out of cards, a slot will be null                                                
                .ToArray();

            // First, if I can buy a card that gives victory points, I always do.
            foreach(var card in allFaceUpCards.Concat(me.ReservedCards)
                                              .Where(c => c.VictoryPoints > 0)
                                              .OrderByDescending(c => c.VictoryPoints)
                                              .Where(CanBuy))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            // I look at all the cards I can see and choose one that looks the best in terms of accessibility
            var bestCardStudy = AnalyseCards(me, allFaceUpCards.Concat(me.ReservedCards), gameState)
                   .OrderBy(s => s.DifficultyRating)
                   .FirstOrDefault();

            // Second, if the most accessible card is purchasable, I buy it.
            if (CanBuy(bestCardStudy.Card))
            {
                return new BuyCard(bestCardStudy.Card, BuyCard.CreateDefaultPaymentOrNull(me, bestCardStudy.Card));
            }

            // Third, I try and get rid of reserved cards.
            foreach (var card in me.ReservedCards.Where(CanBuy))
            {
                return new BuyCard(card, BuyCard.CreateDefaultPaymentOrNull(me, card));
            }

            // Fourth, I check if I've got loads of coins and if so, I buy any card I can afford
            if (me.Purse.Sum > 8)
            {
                foreach (var card in allFaceUpCards.Where(CanBuy))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                    return new BuyCard(card, payment);
                } 
            }

            // Fifth, I top up my coins, favouring colours needed by the most accessible card.
            var coloursAvailable = gameState.TokensAvailable.Colours(includeGold: false).ToList();
            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Sum, 3), coloursAvailable.Count);           

            if (coinsCountICanTake > 0)
            {
                if (bestCardStudy != null)
                {
                    var coloursNeeded = bestCardStudy.Deficit.Colours().ToArray();
                    coloursAvailable = coloursAvailable.OrderByDescending(col => coloursNeeded.Contains(col)).ToList();
                }
                else
                {
                    coloursAvailable.Shuffle();
                }
                var transaction = new Pool();
                if (bestCardStudy.Deficit.Colours().Any(col => bestCardStudy.Deficit[col] >= 2) && coinsCountICanTake > 1)
                {
                    var neededColour = bestCardStudy.Deficit.Colours().First(col => bestCardStudy.Deficit[col] >= 2);
                    if (gameState.TokensAvailable[neededColour] > 3)
                    {
                        transaction[neededColour] = 2;
                        return new TakeTokens(transaction);
                    }
                }
                foreach (var colour in coloursAvailable.Take(coinsCountICanTake)) transaction[colour] = 1;
                return new TakeTokens(transaction);
            }

            // Sixth, if it comes to it, I just reserve the best looking card I can see.
            if(!me.ReservedCards.Contains(bestCardStudy.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(bestCardStudy.Card);
            }

            // Seventh, If I've already reserved the best looking card, I take a gamble.
            var action = ChooseRandomCardOrNull(gameState);
            if (action != null) return action;

            // Lastly, if I think I can't do anything at all I just pass the turn.
            return new NoAction();
        }

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(Player me, IEnumerable<Card> cards, GameState state)
        {
            foreach (var card in cards)
            {
                var cost = card.Cost;
                if (cost == null) continue;
                var deficit = new Pool();
                int scarcity = 0;
                foreach(var colour in cost.Colours())
                {
                    deficit[colour] = Math.Max(0, cost[colour] - me.Budget[colour]);
                    scarcity += Math.Max(0, deficit[colour] - state.TokensAvailable[colour]);
                }
                var rating = deficit.Sum + scarcity;
                yield return new CardFeasibilityStudy { Deficit = deficit, DifficultyRating = rating, Card = card };
            }
        }

        private ReserveFaceDownCard ChooseRandomCardOrNull(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;

            var colourToGiveUp = me.Purse.Colours(includeGold: false).FirstOrDefault();
            var firstTier = gameState.Tiers.Single(t => t.Tier == 1);
            if (firstTier.HasFaceDownCardsRemaining)
            {
                return new ReserveFaceDownCard(1, colourToGiveUp);
            }
            return null;
        }

        private class CardFeasibilityStudy
        {
            public int DifficultyRating { get; set; }
            public IPool Deficit { get; set; }
            public Card Card { get; set; }
        }
    }
}
