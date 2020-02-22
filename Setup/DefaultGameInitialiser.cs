using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;
using Splendor.Core.Domain;

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
            var startingTokensPerColour 
                = players == 4 ? 7 
                : players == 3 ? 5 
                : players == 2 ? 4 
                : throw new ArgumentOutOfRangeException("Only 2,3, or 4 players are allowed.");

            var nobleCount = players + 1;

            var tokenBank = new Dictionary<TokenColour, int>();

            tokenBank[TokenColour.Gold] = 5;
            tokenBank[TokenColour.White] = startingTokensPerColour;
            tokenBank[TokenColour.Red] = startingTokensPerColour;
            tokenBank[TokenColour.Blue] = startingTokensPerColour;
            tokenBank[TokenColour.Green] = startingTokensPerColour;
            tokenBank[TokenColour.Black] = startingTokensPerColour;

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

            return new GameState(tokenBank, nobles, boardTiers.ToArray(), playerList.ToArray());
        }
    }
}
