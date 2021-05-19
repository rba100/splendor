
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public record GameState
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
            CurrentPlayerIndex = currentPlayerIndex;
        }

        public IPool Bank { get; init; }
        public ICollection<Noble> Nobles { get; init; }
        public IReadOnlyCollection<BoardTier> Tiers { get; init; }
        public IReadOnlyCollection<Player> Players { get; init; }
        public Player CurrentPlayer => Players.Skip(CurrentPlayerIndex).First();

        public int CurrentPlayerIndex { get; init; }

        public GameState CloneWithIncrementedCurrentPlayer()
        {
            return this with { CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count };
        }

        /// <summary>
        /// Clones the state, but with the player collection modified to
        /// include the supplied player in place of any player with the same Player.Name.
        /// </summary>
        public GameState WithUpdatedPlayerByName(Player player)
        {
            var nextPlayers = Players.Select(p => p.Name == player.Name ? player : p).ToList();
            return this with { Players = nextPlayers };
        }
    }
}
