
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public class GameState
    {
        public GameState(IPool coinsAvailable,
                         ICollection<Noble> nobles, 
                         IReadOnlyCollection<BoardTier> tiers,
                         IReadOnlyCollection<Player> players, 
                         int currentPlayerIndex)
        {
            Bank = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
            Tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));
            Players = players ?? throw new ArgumentNullException(nameof(players));
            _currentPlayerIndex = currentPlayerIndex;
        }

        public IPool Bank { get; }
        public ICollection<Noble> Nobles { get; }
        public IReadOnlyCollection<BoardTier> Tiers { get; }
        public IReadOnlyCollection<Player> Players { get; }
        public Player CurrentPlayer => Players.Skip(_currentPlayerIndex).First();

        private readonly int _currentPlayerIndex;

        public GameState Clone(IPool withTokensAvailable = null,
                               ICollection<Noble> withNobles = null,
                               IReadOnlyCollection<BoardTier> withTiers = null,
                               IReadOnlyCollection<Player> withPlayers = null, 
                               int? currentPlayerIndex = null)
        {
            return new GameState(
                withTokensAvailable ?? Bank,
                withNobles ?? Nobles,
                withTiers ?? Tiers,
                withPlayers ?? Players,
                currentPlayerIndex.HasValue ? currentPlayerIndex.Value : _currentPlayerIndex);
        }

        public GameState CloneWithIncrementedCurrentPlayer()
        {
            return new GameState(
                Bank,
                Nobles,
                Tiers,
                Players,
                (_currentPlayerIndex+1) % Players.Count);
        }

        /// <summary>
        /// Clones the state, but with the player collection modified to
        /// include the supplied player in place of any player with the same Player.Name.
        /// </summary>
        public GameState CloneWithPlayerReplacedByName(Player player)
        {
            var nextPlayers = Players.Select(p => p.Name == player.Name ? player : p).ToList();
            return Clone(withPlayers: nextPlayers);
        }
    }
}
