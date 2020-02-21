using Splendor.Core.Actions;

using System;
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
            bool CanBuy(Card card) => BuyCard.CanBuyCard(me, gameState, card);

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

            // Buy a card from my hand if possible
            foreach (var card in me.ReservedCards.Where(CanBuy))
            {
                return new BuyCard(card);
            }

            // If I have 10 coins buy any card I can at all
            if (me.Purse.Values.Sum() == 10)
            {
                foreach (var card in allFaceUpCards.Where(CanBuy))
                {
                    var payment = BuyCard.CreateDefaultPaymentOrNull(me, card);
                    return new BuyCard(card, payment);
                } 
            }

            // Once in a while reserve a random card
            if(_random.Next(6) == 0)
            {
                var ac = ChooseFaceDownCard(gameState);
                if (ac != null) return ac;
            }

            var colours = gameState.CoinsAvailable.Where(kvp => kvp.Value > 0 && kvp.Key != CoinColour.Gold).Select(c=>c.Key).ToList();
            var count = Math.Min(Math.Min(10 - me.Purse.Values.Sum(), 3), colours.Count);
            
            if (count > 0)
            {
                colours.Shuffle();
                var transaction = Utility.CreateEmptyTransaction();
                foreach (var colour in colours.Take(count)) transaction[colour] = 1;
                return new TakeCoins(transaction);
            }

            // Do a reserve
            var action = ChooseFaceDownCard(gameState);
            if (action != null) return action;

            // Give up
            return new NoAction();
        }

        private ReserveFaceDownCard ChooseFaceDownCard(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            var colourToGiveUp = me.Purse.Where(kvp => kvp.Value > 0 && kvp.Key != CoinColour.Gold).Select(kvp => kvp.Key).FirstOrDefault();

            if (me.ReservedCards.Count == 3) return null;

            //var myVictoryPoints = me.VictoryPoints();
            //if (myVictoryPoints > 9 && gameState.Tiers.Last().FaceDownCards.Count > 0)
            //{
            //    return new ReserveFaceDownCard(gameState.Tiers.Last().FaceDownCards.Peek().Tier, colourToGiveUp);
            //}
            //if (myVictoryPoints > 4 && gameState.Tiers.Skip(1).First().FaceDownCards.Count > 0)
            //{
            //    return new ReserveFaceDownCard(gameState.Tiers.Skip(1).First().FaceDownCards.Peek().Tier, colourToGiveUp);
            //}
            if (gameState.Tiers.First().FaceDownCards.Count > 0)
            {
                return new ReserveFaceDownCard(gameState.Tiers.First().FaceDownCards.Peek().Tier, colourToGiveUp);
            }
            return null;
        }

        private ReserveFaceDownCard ChooseFaceUpCard(GameState gameState)
        {
            var me = gameState.CurrentPlayer;
            if (me.ReservedCards.Count == 3) return null;

            var myVictoryPoints = me.VictoryPoints();
            if (myVictoryPoints > 9 && gameState.Tiers.Last().FaceDownCards.Count > 0)
            {
                return new ReserveFaceDownCard(gameState.Tiers.Last().FaceDownCards.Peek().Tier);
            }
            if (myVictoryPoints > 4 && gameState.Tiers.Skip(1).First().FaceDownCards.Count > 0)
            {
                return new ReserveFaceDownCard(gameState.Tiers.Skip(1).First().FaceDownCards.Peek().Tier);
            }
            if (gameState.Tiers.First().FaceDownCards.Count > 0)
            {
                return new ReserveFaceDownCard(gameState.Tiers.First().FaceDownCards.Peek().Tier);
            }
            return null;
        }
    }
}
