using Splendor.Core.Actions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.AI
{
    public class StupidSplendorAi : ISpendorAi
    {
        private readonly Random _random = new Random();

        public string Name { get; private set; }

        public StupidSplendorAi(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IAction ChooseAction(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var allFaceUpCards = gameState.Tiers.SelectMany(t => t.ColumnSlots)
                                                .Select(s => s.Value)
                                                .Where(card => card != null)
                                                .ToArray();

            // Buy a victory point card if possible
            foreach(var card in allFaceUpCards.Concat(me.ReservedCards)
                                              .Where(c => c.VictoryPoints > 0)
                                              .OrderByDescending(c => c.VictoryPoints)
                                              .Where(CanBuy))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            var bestCardStudy = AnalyseCards(me, allFaceUpCards.Concat(me.ReservedCards), gameState)
                   .OrderBy(s => s.DifficultyRating)
                   .FirstOrDefault();

            // Buy favourite card if possible
            if (CanBuy(bestCardStudy.Card))
            {
                return new BuyCard(bestCardStudy.Card, BuyCard.CreateDefaultPaymentOrNull(me, bestCardStudy.Card));
            }

            // Buy a card from my hand if possible
            foreach (var card in me.ReservedCards.Where(CanBuy))
            {
                return new BuyCard(card, BuyCard.CreateDefaultPaymentOrNull(me, card));
            }

            // If I have 9 or more coins buy any card I can
            if (me.Purse.Values.Sum() > 8)
            {
                foreach (var card in allFaceUpCards.Where(CanBuy))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                    return new BuyCard(card, payment);
                } 
            }

            // Once in a while reserve a random card
            if(_random.Next(8) == 0)
            {
                var ac = ChooseRandomCardOrNull(gameState);
                if (ac != null) return ac;
            }

            // Take some coins
            var coloursAvailable = gameState.CoinsAvailable.Where(kvp => kvp.Value > 0 && kvp.Key != CoinColour.Gold).Select(c=>c.Key).ToList();
            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Values.Sum(), 3), coloursAvailable.Count);           

            if (coinsCountICanTake > 0)
            {
                if (bestCardStudy != null)
                {
                    var coloursNeeded = bestCardStudy.Deficit.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToList();
                    coloursAvailable = coloursAvailable.OrderByDescending(col => coloursNeeded.Contains(col)).ToList();
                }
                else
                {
                    coloursAvailable.Shuffle();
                }
                var transaction = Utility.CreateEmptyTransaction();
                foreach (var colour in coloursAvailable.Take(coinsCountICanTake)) transaction[colour] = 1;
                return new TakeCoins(transaction);
            }

            // Do a reserve
            if(!me.ReservedCards.Contains(bestCardStudy.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(bestCardStudy.Card);
            }

            var action = ChooseRandomCardOrNull(gameState);
            if (action != null) return action;

            // Give up
            return new NoAction();
        }

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(Player me, IEnumerable<Card> cards, GameState state)
        {
            var budget = me.Purse.CreateCopy().MergeWith(me.GetDiscount());
            foreach (var card in cards)
            {
                var cost = card.Cost;
                if (cost == null) continue;
                var deficit = Utility.CreateEmptyTransaction();
                int scarcity = 0;
                foreach(var colour in cost.Keys)
                {
                    deficit[colour] = Math.Max(0, cost[colour] - budget[colour]);
                    scarcity += Math.Min(0, deficit[colour] - state.CoinsAvailable[colour]);
                }
                var rating = deficit.Values.Sum() + scarcity;
                yield return new CardFeasibilityStudy { Deficit = deficit, DifficultyRating = rating, Card = card };
            }
        }

        private ReserveFaceDownCard ChooseRandomCardOrNull(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;

            var colourToGiveUp = me.Purse.Where(kvp => kvp.Value > 0 && kvp.Key != CoinColour.Gold).Select(kvp => kvp.Key).FirstOrDefault();
            var firstTier = gameState.Tiers.Single(t => t.Tier == 1);
            if (firstTier.FaceDownCards.Count > 0)
            {
                return new ReserveFaceDownCard(1, colourToGiveUp);
            }
            return null;
        }

        private class CardFeasibilityStudy
        {
            public int DifficultyRating { get; set; }
            public IDictionary<CoinColour, int> Deficit { get; set; }
            public Card Card { get; set; }
        }
    }
}
