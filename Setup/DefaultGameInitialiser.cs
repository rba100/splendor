using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor
{
    public class DefaultGameInitialiser : IGameInitialiser
    {
        private readonly IGameDataSource _gameDataSource;

        private const int columns = 4;

        public DefaultGameInitialiser(IGameDataSource gameDataSource)
        {
            _gameDataSource = gameDataSource ?? throw new ArgumentNullException(nameof(gameDataSource));
        }
        
        public GameState Create(int players)
        {
            var startingCoinsAvailable = players > 3 ? 7 : 5;
            var nobleCount = players + 1;

            var coinBank = new Dictionary<CoinColour, int>();

            coinBank[CoinColour.Gold] = 5;
            coinBank[CoinColour.White] = startingCoinsAvailable;
            coinBank[CoinColour.Red] = startingCoinsAvailable;
            coinBank[CoinColour.Blue] = startingCoinsAvailable;
            coinBank[CoinColour.Green] = startingCoinsAvailable;
            coinBank[CoinColour.Black] = startingCoinsAvailable;

            var allNobles = _gameDataSource.AllNobles().ToList();
            allNobles.Shuffle();
            var nobles = allNobles.Take(nobleCount).ToList();

            var boardTiers = new List<BoardTier>();
            var cards = _gameDataSource.AllCards().ToList();
            cards.Shuffle();
            var tiers = cards.Select(c => c.Tier).Distinct();
            foreach(var tier in tiers)
            {
                boardTiers.Add(new BoardTier(tier, cards.Where(c => c.Tier == tier), columns));
            }

            var playerList = new List<Player>();
            for (var i = 0; i < players; i++) playerList.Add(new Player($"Player {i+1}"));

            return new GameState(coinBank, nobles, boardTiers.ToArray(), playerList.ToArray());
        }
    }
}
