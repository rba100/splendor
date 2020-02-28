
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor
{
    public class GameState
    {
        public GameState(IDictionary<TokenColour, int> coinsAvailable, ICollection<Noble> nobles, IReadOnlyCollection<BoardTier> tiers, IReadOnlyCollection<Player> players, Player currentPlayer)
        {
            TokensAvailable = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
            Tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));
            Players = players ?? throw new ArgumentNullException(nameof(players));
            CurrentPlayer = currentPlayer ?? throw new ArgumentNullException(nameof(currentPlayer));
        }

        public IDictionary<TokenColour, int> TokensAvailable { get; private set; }
        public ICollection<Noble> Nobles { get; private set; }
        public IReadOnlyCollection<BoardTier> Tiers { get; private set; }
        public IReadOnlyCollection<Player> Players { get; private set; }
        public Player CurrentPlayer { get; private set; }

        public GameState CopyWith(IDictionary<TokenColour, int> coinsAvailable = null, ICollection<Noble> nobles = null, IReadOnlyCollection<BoardTier> tiers = null, IReadOnlyCollection<Player> players = null, Player currentPlayer = null)
        {
            return new GameState(
                coinsAvailable ?? TokensAvailable,
                nobles ?? Nobles,
                tiers ?? Tiers,
                players ?? Players,
                currentPlayer ?? CurrentPlayer);
        }

        public GameState CopyWithPlayer(Player player)
        {
            var nextPlayers = new List<Player>();
            foreach (var p in Players) if (p.Name == player.Name)
                    nextPlayers.Add(player);
                else nextPlayers.Add(p);

            var cp = CurrentPlayer.Name == player.Name ? player : CurrentPlayer;

            return CopyWith(players: nextPlayers, currentPlayer: cp);
        }
    }
}
