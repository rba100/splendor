using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using TokenPool = System.Collections.Generic.IReadOnlyDictionary<Splendor.TokenColour, int>;

namespace Splendor.Core.AI
{
    public class ObservantStupidSplendorAi : ISpendorAi
    {
        public string Name { get; private set; }

        private AiOptions _options;

        public ObservantStupidSplendorAi(string name, AiOptions options = null)
        {
            Name = name;
            _options = options ?? new AiOptions();
        }

        public IAction ChooseAction(GameState gameState)
        {
            /* PRECALCULATIONS */

            var me = gameState.CurrentPlayer;
            var otherPlayers = gameState.Players.Where(p => p != me).ToArray();
            var myBudget = me.GetDiscount().MergeWith(me.Purse);

            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var coloursForNoble = GetColoursForCheapestNoble(gameState);

            var allFaceUpCards = gameState.Tiers.SelectMany(t => t.ColumnSlots)
                                                .Select(s => s.Value)
                                                .Where(card => card != null)
                                                .ToArray();

            var faceUpAndMyReserved = allFaceUpCards.Concat(me.ReservedCards).ToArray();

            var cardsICanBuy = faceUpAndMyReserved.Where(CanBuy);

            var myOrderedCardStudy = AnalyseCards(myBudget, faceUpAndMyReserved, gameState, coloursForNoble)
                    .OrderBy(s => s.Repulsion)
                    .ThenByDescending(s => coloursForNoble.Contains(s.Card.BonusGiven)).ToArray(); // Costs us nothing to thenby this condition

            var myTargetCard = myOrderedCardStudy.FirstOrDefault();

            CardFeasibilityStudy myNextTargetCard = null;
            if (_options.LooksAhead)
            {
                var targetDiscount = Utility.CreateEmptyTokenPool();
                targetDiscount[myTargetCard.Card.BonusGiven] = 1;
                var nextBudget = myBudget.MergeWith(targetDiscount);
                myNextTargetCard = AnalyseCards(nextBudget, faceUpAndMyReserved.Except(new[] { myTargetCard.Card }).ToArray(), gameState, coloursForNoble)
                    .OrderBy(s => s.Repulsion)
                    .FirstOrDefault();
            }

            /* BEHAVIOUR */

            // Check to see if other players are about to win
            if (_options.IsTheiving) foreach (var otherPlayer in otherPlayers)
                {
                    var score = otherPlayer.VictoryPoints();
                    var otherPlayerDiscount = otherPlayer.GetDiscount();

                    var nobleDangerMap = new List<TokenColour>();
                    foreach (var noble in gameState.Nobles)
                    {
                        TokenPool deficit = otherPlayerDiscount.GetDeficitFor(noble.Cost);
                        if (deficit.SumValues() != 1) continue;
                        nobleDangerMap.Add(deficit.NonZeroColours().Single());
                    }

                    if (score < 10 && !nobleDangerMap.Any()) continue;

                    var riskCards = allFaceUpCards.Where(c => c.VictoryPoints + score + (nobleDangerMap.Contains(c.BonusGiven) ? 3 : 0) >= 15)
                                                  .Where(c => BuyCard.CanAffordCard(otherPlayer, c))
                                                  .ToArray();

                    if (riskCards.Length != 1) continue; // We're screwed if he has more than one winning move.
                    var riskCard = riskCards.Single();
                    if (cardsICanBuy.Contains(riskCard)) return new BuyCard(riskCard, BuyCard.CreateDefaultPaymentOrNull(me, riskCard));
                    if (me.ReservedCards.Count < 3) return new ReserveCard(riskCard);
                }

            // Buy a 2 or greater victory point card if possible
            foreach (var card in cardsICanBuy.Where(c => c.VictoryPoints > 1)
                                             .OrderByDescending(c => c.VictoryPoints))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            // Buy favourite card if possible
            if (cardsICanBuy.Contains(myTargetCard.Card))
            {
                return new BuyCard(myTargetCard.Card, BuyCard.CreateDefaultPaymentOrNull(me, myTargetCard.Card));
            }

            // If I have 9 or more coins buy any reasonable card I can at all
            if (me.Purse.Values.Sum() > 8)
            {
                foreach (var study in myOrderedCardStudy.Where(s => s.Deficit.SumValues() == 0))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, study.Card);
                    return new BuyCard(study.Card, payment);
                }
            }

            // Take some coins - but if there are coins to return then favour reserving a card
            var takeTokens = GetTakeTokens(gameState, myTargetCard, myNextTargetCard);
            if (takeTokens != null && !takeTokens.TokensToReturn.NonZeroColours().Any()) return takeTokens;

            // Do a reserve
            if (!me.ReservedCards.Contains(myTargetCard.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(myTargetCard.Card);
            }

            // Do a random reserve
            var action = ChooseRandomCardOrNull(gameState);
            if (action != null) return action;

            // do the give/take coins if possible
            return takeTokens ?? (IAction)new NoAction();
        }

        private TakeTokens GetTakeTokens(GameState gameState, CardFeasibilityStudy firstChoice, CardFeasibilityStudy secondChoice)
        {
            if (firstChoice == null) return null;

            var me = gameState.CurrentPlayer;

            var coloursAvailable = gameState.TokensAvailable.Where(kvp => kvp.Value > 0 && kvp.Key != TokenColour.Gold).Select(c => c.Key).ToList();
            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Values.Sum(), 3), coloursAvailable.Count);

            if (coinsCountICanTake > 0)
            {
                if (firstChoice != null)
                {
                    var coloursNeeded = firstChoice.Deficit.NonZeroColours().ToList();
                    var coloursNeeded2 = secondChoice?.Deficit.NonZeroColours().ToList();
                    var ordering = coloursAvailable.OrderByDescending(col => coloursNeeded.Contains(col));
                    if (secondChoice != null) ordering = ordering.ThenByDescending(col => coloursNeeded2.Contains(col));
                    coloursAvailable = ordering.ToList();
                }
                else
                {
                    coloursAvailable.Shuffle();
                }
                var transaction = Utility.CreateEmptyTokenPool();
                if (firstChoice.Deficit.Any(kvp => kvp.Value >= 2) && coinsCountICanTake > 1)
                {
                    var neededColour = firstChoice.Deficit.First(kvp => kvp.Value >= 2).Key;
                    if (gameState.TokensAvailable[neededColour] > 3)
                    {
                        transaction[neededColour] = 2;
                        return new TakeTokens(transaction);
                    }
                }
                foreach (var colour in coloursAvailable.Take(coinsCountICanTake)) transaction[colour] = 1;
                return new TakeTokens(transaction);
            }

            // otherwise just swap a single coin towards what we need
            if (coloursAvailable.Count == 0) return null;
            var colourToTake = coloursAvailable.First();
            var colourToGiveBack = me.Purse.NonZeroColours()
                                           .OrderBy(c => c == TokenColour.Gold)
                                           .Where(c => c != colourToTake)
                                           .Cast<TokenColour?>()
                                           .FirstOrDefault();
            if (!colourToGiveBack.HasValue) return null;
            var take = Utility.CreateEmptyTokenPool();
            var give = Utility.CreateEmptyTokenPool();

            take[colourToTake] = 1;
            give[colourToGiveBack.Value] = 1;

            return new TakeTokens(take, give);
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

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(TokenPool budget, Card[] cards, GameState state, TokenColour[] coloursForNoble)
        {
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
                var repulsion = deficit.Values.Sum() + scarcity * 1;
                if (_options.ConsidersNobles && coloursForNoble.Contains(card.BonusGiven)) repulsion -= 1;
                if (coloursNeeded.Contains(card.BonusGiven)) repulsion -= 2;
                if (card.VictoryPoints > 0) repulsion -= 1;

                yield return new CardFeasibilityStudy { Deficit = deficit, Repulsion = repulsion, Card = card };
            }
        }

        private TokenColour[] ColoursByUsefulness(TokenPool budget, IEnumerable<Card> cards)
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

            var firstTier = gameState.Tiers.Single(t => t.Tier == 1);
            if (firstTier.HasFaceDownCardsRemaining)
            {
                return new ReserveFaceDownCard(1);
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
        public bool LooksAhead { get; set; } = true;
        public bool ConsidersNobles { get; set; } = false;
    }
}
