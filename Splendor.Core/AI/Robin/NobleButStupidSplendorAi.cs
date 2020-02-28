using Splendor.Core.Actions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.AI
{
    public class NobleButStupidSplendorAi : ISpendorAi
    {
        public string Name { get; private set; }

        public NobleButStupidSplendorAi(string name)
        {
            Name = name;
        }

        public IAction ChooseAction(GameState gameState)
        {
            var me = gameState.CurrentPlayer;

            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var coloursForNoble = GetColoursForCheapestNoble(gameState);

            var allFaceUpCards = gameState.Tiers.SelectMany(t => t.ColumnSlots)
                                                .Select(s => s.Value)
                                                .Where(card => card != null) 
                                                .ToArray();

            // Buy a victory point card if possible
            foreach (var card in allFaceUpCards.Concat(me.ReservedCards)
                                               .Where(c => c.VictoryPoints > 0)
                                               .OrderByDescending(c => c.VictoryPoints)
                                               .ThenByDescending(c=> coloursForNoble.Contains(c.BonusGiven))
                                               .Where(CanBuy))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            var bestCardStudy = AnalyseCards(me, allFaceUpCards.Concat(me.ReservedCards), gameState, coloursForNoble)
                    .OrderBy(s => s.Repulsion)
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

            // If I have 9 or more coins buy any card I can at all
            if (me.Purse.Values.Sum() > 8)
            {
                foreach (var card in allFaceUpCards.Where(CanBuy))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                    return new BuyCard(card, payment);
                }
            }

            // Take some coins
            var coloursAvailable = gameState.TokensAvailable.Where(kvp => kvp.Value > 0 && kvp.Key != TokenColour.Gold).Select(c => c.Key).ToList();
            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Values.Sum(), 3), coloursAvailable.Count);

            if (coinsCountICanTake > 0)
            {
                if (bestCardStudy != null)
                {
                    var coloursNeeded = bestCardStudy.Deficit.NonZeroColours().ToList();
                    coloursAvailable = coloursAvailable.OrderByDescending(col => coloursNeeded.Contains(col)).ToList();
                }
                else
                {
                    coloursAvailable.Shuffle();
                }
                var transaction = Utility.CreateEmptyTokenPool();
                if(bestCardStudy.Deficit.Any(kvp=>kvp.Value >= 2) && coinsCountICanTake > 1)
                {
                    var neededColour = bestCardStudy.Deficit.First(kvp => kvp.Value >= 2).Key;
                    if (gameState.TokensAvailable[neededColour] > 3)
                    {
                        transaction[neededColour] = 2;
                        return new TakeTokens(transaction);
                    }
                }
                foreach (var colour in coloursAvailable.Take(coinsCountICanTake)) transaction[colour] = 1;
                return new TakeTokens(transaction);
            }

            // Do a reserve
            if (!me.ReservedCards.Contains(bestCardStudy.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(bestCardStudy.Card);
            }

            // Do a reserve
            var action = ChooseRandomCardOrNull(gameState);
            if (action != null) return action;

            // Give up
            return new NoAction();
        }

        private TokenColour[] GetColoursForCheapestNoble(GameState gameState)
        {
            var me = gameState.CurrentPlayer;

            Noble cheapestNoble = null;
            int cheapestNobleDef = 99;
            foreach (var noble in gameState.Nobles)
            {
                var deficitColours = me.Purse.GetDeficitFor(noble.Cost);
                var deficitSum = me.Purse.GetDeficitFor(noble.Cost).SumValues();

                if(deficitSum < cheapestNobleDef)
                {
                    cheapestNobleDef = deficitSum;
                    cheapestNoble = noble;
                }
            }
            return cheapestNoble?.Cost.NonZeroColours() ?? new TokenColour[0];
        }

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(Player me, IEnumerable<Card> cards, GameState state, TokenColour[] favouriteColours)
        {
            Dictionary<TokenColour,int> budget = me.Purse.MergeWith(me.GetDiscount());
            foreach (var card in cards)
            {
                var cost = card.Cost;
                if (cost == null) continue;
                var deficit = Utility.CreateEmptyTokenPool();
                int scarcity = 0;
                foreach (var colour in cost.Keys)
                {
                    deficit[colour] = Math.Max(0, cost[colour] - budget[colour]);
                    scarcity += Math.Max(0, deficit[colour] - state.TokensAvailable[colour]);
                }
                var repulsion = deficit.Values.Sum() + scarcity;
                if (favouriteColours.Contains(card.BonusGiven)) repulsion -= 1;

                yield return new CardFeasibilityStudy { Deficit = deficit, Repulsion = repulsion, Card = card };
            }
        }

        private ReserveFaceDownCard ChooseRandomCardOrNull(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;

            var colourToGiveUp = me.Purse.Where(kvp => kvp.Value > 0 && kvp.Key != TokenColour.Gold).Select(kvp => kvp.Key).FirstOrDefault();
            var firstTier = gameState.Tiers.Single(t => t.Tier == 1);
            if (firstTier.HasFaceDownCardsRemaining)
            {
                return new ReserveFaceDownCard(1, colourToGiveUp);
            }
            return null;
        }

        private class CardFeasibilityStudy
        {
            public int Repulsion { get; set; }
            public IReadOnlyDictionary<TokenColour, int> Deficit { get; set; }
            public Card Card { get; set; }
        }
    }
}
