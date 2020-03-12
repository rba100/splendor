using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.AI
{
    public class ObservantStupidSplendorAi : ISpendorAi
    {
        public string Name { get; }

        private readonly AiOptions _options;

        public ObservantStupidSplendorAi(string name, AiOptions options = null)
        {
            Name = name;
            _options = options ?? new AiOptions();
        }

        public IAction ChooseAction(GameState gameState)
        {
            /* PRE-CALCULATIONS */

            var me = gameState.CurrentPlayer;

            bool CanBuy(Card card) => BuyCard.CanAffordCard(me, card);

            var allFaceUpCards = gameState.Tiers.SelectMany(t => t.ColumnSlots)
                                                .Select(s => s.Value)
                                                .Where(card => card != null)
                                                .ToArray();

            var faceUpAndMyReserved = allFaceUpCards.Concat(me.ReservedCards).ToArray();

            var cardsICanBuy = faceUpAndMyReserved.Where(CanBuy).ToArray();

            var myOrderedCardStudy = AnalyseCards(me, faceUpAndMyReserved, gameState)
                    .OrderBy(s => s.Repulsion).ToArray();

            var myTargetCard = myOrderedCardStudy.FirstOrDefault();

            /* BEHAVIOUR */

            // Check to see if a player can win (including me)
            if (_options.IsTheiving) foreach (var player in gameState.Players.OrderByDescending(p => p == me))
            {
                var score = player.VictoryPoints;

                var nobleDangerMap = new List<TokenColour>();
                foreach (var noble in gameState.Nobles)
                {
                    var deficit = player.Bonuses.DeficitFor(noble.Cost);
                    if (deficit.Sum != 1) continue;
                    nobleDangerMap.Add(deficit.Colours().Single());
                }

                if (score < 10 && !nobleDangerMap.Any()) continue;

                var winCards = allFaceUpCards.Where(c => c.VictoryPoints + score + (nobleDangerMap.Contains(c.BonusGiven) ? 3 : 0) >= 15)
                                             .Where(c => BuyCard.CanAffordCard(player, c))
                                             .OrderByDescending(c => c.VictoryPoints + (nobleDangerMap.Contains(c.BonusGiven) ? 3 : 0))
                                             .ToArray();

                if (winCards.Length == 0) continue;
                if (player != me && winCards.Length > 1) break; // We're screwed if he has more than one winning move.
                var winningCard = winCards.First();
                if (cardsICanBuy.Contains(winningCard)) return new BuyCard(winningCard, BuyCard.CreateDefaultPaymentOrNull(me, winningCard));
                if (player != me && me.ReservedCards.Count < 3) return new ReserveCard(winningCard);
            }

            // Buy a 2 or greater victory point card if possible
            if(_options.Greedy) foreach (var card in cardsICanBuy.Where(c => c.VictoryPoints > 1)
                                                                 .OrderByDescending(c => c.VictoryPoints))
            {
                var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                return new BuyCard(card, payment);
            }

            // Buy favourite card if possible
            if (myTargetCard?.DeficitWithGold == 0)
            {
                return new BuyCard(myTargetCard.Card, BuyCard.CreateDefaultPaymentOrNull(me, myTargetCard.Card));
            }

            // If I have 9 or more coins, or there aren't many coins left to take, buy any reasonable card I can at all
            if (me.Purse.Sum > 8 || gameState.Bank.Colours(includeGold: false).Count() < 3)
            {
                foreach (var study in myOrderedCardStudy.Where(s => s.DeficitWithGold == 0))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, study.Card);
                    return new BuyCard(study.Card, payment);
                }
            }

            /* more precalc */
            CardFeasibilityStudy myNextTargetCard = null;
            if (_options.LooksAhead && myTargetCard != null)
            {
                var nextCards = me.CardsInPlay.ToList();
                nextCards.Add(myTargetCard.Card);
                var nextMe = me.Clone(withCardsInPlay: nextCards);
                myNextTargetCard = AnalyseCards(nextMe, faceUpAndMyReserved.Except(new[] { myTargetCard.Card }).ToArray(), gameState)
                    .OrderBy(s => s.Repulsion)
                    .FirstOrDefault();
            }

            // Take some coins - but if there are coins to return then favour reserving a card
            var takeTokens = GetTakeTokens(gameState, myTargetCard, myNextTargetCard);
            if (takeTokens != null && !takeTokens.TokensToReturn.Colours().Any()) return takeTokens;

            // Do a reserve
            if (myTargetCard != null && !me.ReservedCards.Contains(myTargetCard.Card) && me.ReservedCards.Count < 3)
            {
                return new ReserveCard(myTargetCard.Card);
            }

            // Do a random reserve if not maxxed out.
            var action = ChooseRandomCardOrNull(gameState);
            if (action != null && _options.RandomReserves) return action;

            // do the give/take coins if possible
            return takeTokens ?? (IAction)new NoAction();
        }

        private TakeTokens GetTakeTokens(GameState gameState, CardFeasibilityStudy firstChoice, CardFeasibilityStudy secondChoice)
        {
            if (firstChoice == null) return null;

            var me = gameState.CurrentPlayer;

            var coloursByPreference = gameState.Bank.Colours(includeGold: false).ToList();

            var coinsCountICanTake = Math.Min(Math.Min(10 - me.Purse.Sum, 3), coloursByPreference.Count);


            var coloursNeeded = firstChoice.Deficit.Colours().ToList();
            var coloursNeeded2 = secondChoice?.Deficit.Colours().ToList();
            var ordering = coloursByPreference.OrderByDescending(col => coloursNeeded.Contains(col));
            if (secondChoice != null) ordering = ordering.ThenByDescending(col => coloursNeeded2.Contains(col));
            coloursByPreference = ordering.ToList();

            if (coinsCountICanTake > 0)
            {
                var deficitColours = firstChoice.Deficit.Colours().ToArray();

                var transaction = new Pool();

                if (_options.CanTakeTwo
                    && deficitColours.Length == 1
                    && deficitColours.Any(col => firstChoice.Deficit[col] >= 2)
                    && coinsCountICanTake > 1)
                {
                    var neededColour = deficitColours.Single();
                    if (gameState.Bank[neededColour] > 3)
                    {
                        transaction[neededColour] = 2;
                        return new TakeTokens(transaction);
                    }
                }
                foreach (var colour in coloursByPreference.Take(coinsCountICanTake)) transaction[colour] = 1;
                return new TakeTokens(transaction);
            }

            // otherwise just swap a single coin towards what we need
            if (coloursByPreference.Count == 0) return null;
            var colourToTake = coloursByPreference.First();
            var colourToGiveBack = me.Purse.Colours()
                                           .OrderBy(c => c == TokenColour.Gold || c == colourToTake)
                                           .Cast<TokenColour?>()
                                           .FirstOrDefault();
            if (!colourToGiveBack.HasValue) return null;
            var take = new Pool();
            var give = new Pool();

            take[colourToTake] = 1;
            give[colourToGiveBack.Value] = 1;

            return new TakeTokens(take, give);
        }

        private IEnumerable<CardFeasibilityStudy> AnalyseCards(Player player, Card[] cards, GameState state)
        {
            var newColourNobility = ColourBiasForNobles(player, state);
            var bonusDesirability = GetBonusDesirability(player, cards);

            foreach (var card in cards)
            {
                yield return GetCardStudy(card, state.CurrentPlayer, state.Bank, bonusDesirability, newColourNobility);
            }
        }

        private CardFeasibilityStudy GetCardStudy(Card card,
                                                  Player player,
                                                  IPool bank,
                                                  IPool bonusDesirability,
                                                  Dictionary<TokenColour, decimal> newColourNobility)
        {
            var deficit = player.Budget.DeficitFor(card.Cost);
            var deficitSum = Math.Max(0, deficit.Sum - player.Purse.Gold);
            var scarcity = bank.DeficitFor(deficit).Sum;
            var repulsion = 0m
                + deficitSum 
                + _options.Biases.FromScarcity(scarcity)
                + _options.Biases.FromVictoryPoints(card.VictoryPoints)
                + _options.Biases.FromCardBonus(bonusDesirability, card.BonusGiven)
                + newColourNobility[card.BonusGiven];

            return new CardFeasibilityStudy { Card = card, Deficit = deficit, Repulsion = repulsion, DeficitWithGold = deficitSum };
        }

        private Dictionary<TokenColour, decimal> ColourBiasForNobles(Player player, GameState state)
        {
            var biases = Utility.AllColours().ToDictionary(c => c, c => 0m);
            if (!_options.LooksAtNobles) return biases;
            var bonuses = player.Bonuses;
            var noblesAsDeficits = state.Nobles.Select(n => bonuses.DeficitFor(n.Cost)).ToArray();
            var coloursPresent = noblesAsDeficits.SelectMany(d => d.Colours()).Distinct();
            foreach (var colour in coloursPresent)
            {
                var noblesWithThatColour = noblesAsDeficits.Where(d => d[colour] > 0).ToArray();
                var sharedBy = noblesWithThatColour.Length;
                var distance = noblesWithThatColour.Select(d => d.Sum).Min();
                biases[colour] += sharedBy * _options.Biases.NobleColourSharedBias;
                biases[colour] += distance * _options.Biases.NobleColourCloseBias;
            }
            return biases;
        }

        private IPool GetBonusDesirability(Player player, Card[] cards)
        {
            var pool = new Pool();
            foreach(var card in cards.Where(c=>c.VictoryPoints > 1))
            {
                var colours = player.Bonuses.DeficitFor(card.Cost).Colours();
                foreach(var colour in colours) pool[colour]++;
            }
            return pool;
        }

        private ReserveFaceDownCard ChooseRandomCardOrNull(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;
            int tier = me.VictoryPoints < 5 ? 1 : me.VictoryPoints < 10 ? 2 : 3;

            var firstTier = gameState.Tiers.Single(t => t.Tier == tier);
            if (firstTier.HasFaceDownCardsRemaining)
            {
                return new ReserveFaceDownCard(tier);
            }
            return null;
        }

        private class CardFeasibilityStudy
        {
            public decimal Repulsion { get; set; }
            public IPool Deficit { get; set; }
            public int DeficitWithGold { get; set; }
            public Card Card { get; set; }
        }
    }

    public class AiOptions
    {
        public bool IsTheiving { get; set; } = true;
        public bool LooksAhead { get; set; } = true;
        public bool CanTakeTwo { get; set; } = true;
        public bool LooksAtNobles { get; set; } = true;

        /// <summary>
        /// Performs a random reserve as a last resort.
        /// </summary>
        public bool RandomReserves { get; set; } = true;
        public bool PhasesGame { get; set; } = false;     

        /// <summary>
        /// Buys a victory point card if able, ignoring other actions.
        /// </summary>
        public bool Greedy { get; set; } = false;

        public BiasValues Biases { get; } = new BiasValues();
    }

    public class BiasValues
    {
        public decimal NobleColourSharedBias { get; set; } = -0.9m;
        public decimal NobleColourCloseBias { get; set; } = -0.2m;
        // Bias values modify 'repulsion'. I.e. good things have low or negative values
        public Func<int, decimal> FromVictoryPoints { get; set; } = vp => -vp * 0.5m;
        public Func<int, decimal> FromScarcity { get; set; } = s => s * 10m;
        public Func<IPool, TokenColour, decimal> FromCardBonus { get; set; } = (cr,col) => -cr[col] * 0.5m;
    }
}
