
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class GameState
    {
        public GameState(IReadOnlyDictionary<TokenColour, int> coinsAvailable, ICollection<Noble> nobles, IReadOnlyCollection<BoardTier> tiers, IReadOnlyCollection<Player> players, int currentPlayerIndex)
        {
            TokensAvailable = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
            Tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));
            Players = players ?? throw new ArgumentNullException(nameof(players));
            _currentPlayerIndex = currentPlayerIndex;
        }

        public IReadOnlyDictionary<TokenColour, int> TokensAvailable { get; private set; }
        public ICollection<Noble> Nobles { get; private set; }
        public IReadOnlyCollection<BoardTier> Tiers { get; private set; }
        public IReadOnlyCollection<Player> Players { get; private set; }
        public Player CurrentPlayer => Players.Skip(_currentPlayerIndex).First();

        private int _currentPlayerIndex = 0;

        public GameState Clone(IReadOnlyDictionary<TokenColour, int> withTokensAvailable = null, ICollection<Noble> withNobles = null, IReadOnlyCollection<BoardTier> withTiers = null, IReadOnlyCollection<Player> withPlayers = null, int? currentPlayerIndex = null)
        {
            return new GameState(
                withTokensAvailable ?? TokensAvailable,
                withNobles ?? Nobles,
                withTiers ?? Tiers,
                withPlayers ?? Players,
                currentPlayerIndex.HasValue ? currentPlayerIndex.Value : _currentPlayerIndex);
        }

        public GameState CloneWithIncrementedCurrentPlayer()
        {
            return new GameState(
                TokensAvailable,
                Nobles,
                Tiers,
                Players,
                (_currentPlayerIndex+1) % Players.Count);
        }

        public GameState CloneWithPlayerReplacedByName(Player player)
        {
            var nextPlayers = new List<Player>();
            foreach (var p in Players) if (p.Name == player.Name)
                    nextPlayers.Add(player);
                else nextPlayers.Add(p);

            var cp = CurrentPlayer.Name == player.Name ? player : CurrentPlayer;

            return Clone(withPlayers: nextPlayers);
        }
    }
}
