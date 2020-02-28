using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.AI
{
    public class InvestingStupidSplendorAi : ISpendorAi
    {
        public string Name { get; private set; }

        private AiOptions _options;

        public InvestingStupidSplendorAi(string name, AiOptions options = null)
        {
            Name = $"Investing {name}";
            _options = options ?? new AiOptions();
        }

        public IAction ChooseAction(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            var otherPlayers = gameState.Players.Where(p => p != me).ToArray();

            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var coloursForNoble = GetColoursForCheapestNoble(gameState);

            var allFaceUpCards = gameState.Tiers.SelectMany(t => t.ColumnSlots)
                                                .Select(s => s.Value)
                                                .Where(card => card != null)
                                                .ToArray();

            var faceUpAndMyReserved = allFaceUpCards.Concat(me.ReservedCards).ToArray();

            var myOrderedCardStudy = AnalyseCards(me, faceUpAndMyReserved, gameState, coloursForNoble)
                    .OrderBy(s => s.Repulsion)
                    .ThenByDescending(s => coloursForNoble.Contains(s.Card.BonusGiven)).ToArray(); // Costs us nothing to thenby this condition

            var myTargetCard = myOrderedCardStudy.FirstOrDefault();

            // Buy a big victory point card if possible
            foreach (var card in allFaceUpCards.Concat(me.ReservedCards)
                                               .Where(c => c.VictoryPoints > 1)
                                               .OrderByDescending(c => c.VictoryPoints)
                                               .Where(CanBuy))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            // Check to see if other players are about to win
            foreach (var otherPlayer in otherPlayers)
            {
                if (!_options.IsTheiving) break;
                var score = otherPlayer.VictoryPoints();
                var riskCards = allFaceUpCards.Where(c => c.VictoryPoints + score >= 15)
                                              .Where(c => BuyCard.CanAffordCard(otherPlayer, c))
                                              .ToArray();
                if (riskCards.Length != 1) continue; // We're screwed if he has more than one winning move.
                var riskCard = riskCards.Single();
                if (CanBuy(riskCard)) return new BuyCard(riskCard, BuyCard.CreateDefaultPaymentOrNull(me, riskCard));
                if (me.ReservedCards.Count < 3) return new ReserveCard(riskCard);
            }

            // Buy favourite card if possible
            if (CanBuy(myTargetCard.Card))
            {
                return new BuyCard(myTargetCard.Card, BuyCard.CreateDefaultPaymentOrNull(me, myTargetCard.Card));
            }

            // If I have 9 or more coins buy any card I can at all
            if (me.Purse.Values.Sum() > 8)
            {
                foreach (var study in myOrderedCardStudy.Where(s=>s.Deficit.SumValues() == 0))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, study.Card);
                    return new BuyCard(study.Card, payment);
                }
            }

            // Take some coins
            var coloursAvailable = gameState.TokensAvailable.Where(kvp => kvp.Value > 0 && kvp.Key != TokenColour.Gold).Select(c => c.Key).ToList();
            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Values.Sum(), 3), coloursAvailable.Count);

            if (coinsCountICanTake > 0)
            {
                if (myTargetCard != null)
                {
                    var coloursNeeded = myTargetCard.Deficit.NonZeroColours().ToList();
                    coloursAvailable = coloursAvailable.OrderByDescending(col => coloursNeeded.Contains(col)).ToList();
                }
                else
                {
                    coloursAvailable.Shuffle();
                }
                var transaction = Utility.CreateEmptyTokenPool();
                if (myTargetCard.Deficit.Any(kvp => kvp.Value >= 2) && coinsCountICanTake > 1)
                {
                    var neededColour = myTargetCard.Deficit.First(kvp => kvp.Value >= 2).Key;
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
            if (!me.ReservedCards.Contains(myTargetCard.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(myTargetCard.Card);
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

                if (deficitSum < cheapestNobleDef)
                {
                    cheapestNobleDef = deficitSum;
                    cheapestNoble = noble;
                }
            }
            return cheapestNoble?.Cost.NonZeroColours() ?? new TokenColour[0];
        }

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(Player player, Card[] cards, GameState state, TokenColour[] coloursForNoble)
        {
            var budget = player.Purse.MergeWith(player.GetDiscount());
            var coloursNeeded = ColoursByUsefulness(budget, cards);

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
                if (_options.ConsidersNobles && coloursForNoble.Contains(card.BonusGiven)) repulsion -= 1;
                if (coloursNeeded.Contains(card.BonusGiven)) repulsion -= 3;

                yield return new CardFeasibilityStudy { Deficit = deficit, Repulsion = repulsion, Card = card };
            }
        }

        private TokenColour[] ColoursByUsefulness(IDictionary<TokenColour, int> budget, IEnumerable<Card> cards)
        {
            var colourChart = Utility.CreateEmptyTokenPool();

            foreach (var card in cards)
            {
                var deficit = budget.GetDeficitFor(card.Cost);
                foreach (var colour in deficit.NonZeroColours()) colourChart[colour] += 1;
            }

            return colourChart
                .Where(kvp => kvp.Key != TokenColour.Gold)
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        private ReserveFaceDownCard ChooseRandomCardOrNull(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;

            var colourToGiveUp = me.Purse.Where(kvp => kvp.Value > 0 && kvp.Key != TokenColour.Gold).Select(kvp => kvp.Key).FirstOrDefault();
            var firstTier = gameState.Tiers.Single(t => t.Tier == 1);
            if (firstTier.FaceDownCards.Count > 0)
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

    public class AiOptions
    {
        public bool IsTheiving { get; set; } = true;
        public bool ConsidersNobles { get; set; } = true;
    }
}
