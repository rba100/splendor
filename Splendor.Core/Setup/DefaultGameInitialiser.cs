using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public class DefaultGameInitialiser : IGameInitialiser
    {
        private readonly IGameDataSource _gameDataSource;

        private const int columns = 4;

        public DefaultGameInitialiser(IGameDataSource gameDataSource)
        {
            _gameDataSource = gameDataSource ?? throw new ArgumentNullException(nameof(gameDataSource));
        }
        
        public GameState Create(IEnumerable<string> playerNames)
        {
            var playerList = new List<Player>();
            foreach (var playerName in playerNames) playerList.Add(new Player(playerName));
            var playerCount = playerList.Count();

            var startingTokensPerColour 
                = playerCount == 4 ? 7 
                : playerCount == 3 ? 5 
                : playerCount == 2 ? 4 
                : throw new ArgumentOutOfRangeException("Only 2,3, or 4 players are allowed.");

            var nobleCount = playerCount + 1;

            var tokenBank = new Pool();

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

            return new GameState(tokenBank, nobles, boardTiers.ToArray(), playerList.ToArray(), 0);
        }
    }
}
