
using System;
using System.Collections.Generic;

namespace Splendor
{
    public class GameState
    {
        public GameState(IReadOnlyDictionary<TokenColour, int> coinsAvailable, ICollection<Noble> nobles, IReadOnlyCollection<BoardTier> tiers, IReadOnlyCollection<Player> players, Player currentPlayer)
        {
            TokensAvailable = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
            Tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));
            Players = players ?? throw new ArgumentNullException(nameof(players));
            CurrentPlayer = currentPlayer ?? throw new ArgumentNullException(nameof(currentPlayer));
        }

        public IReadOnlyDictionary<TokenColour, int> TokensAvailable { get; private set; }
        public ICollection<Noble> Nobles { get; private set; }
        public IReadOnlyCollection<BoardTier> Tiers { get; private set; }
        public IReadOnlyCollection<Player> Players { get; private set; }
        public Player CurrentPlayer { get; private set; }

        public GameState Clone(IReadOnlyDictionary<TokenColour, int> withTokensAvailable = null, ICollection<Noble> withNobles = null, IReadOnlyCollection<BoardTier> withTiers = null, IReadOnlyCollection<Player> withPlayers = null, Player withCurrentPlayer = null)
        {
            return new GameState(
                withTokensAvailable ?? TokensAvailable,
                withNobles ?? Nobles,
                withTiers ?? Tiers,
                withPlayers ?? Players,
                withCurrentPlayer ?? CurrentPlayer);
        }

        public GameState CloneWithPlayerReplacedByName(Player player)
        {
            var nextPlayers = new List<Player>();
            foreach (var p in Players) if (p.Name == player.Name)
                    nextPlayers.Add(player);
                else nextPlayers.Add(p);

            var cp = CurrentPlayer.Name == player.Name ? player : CurrentPlayer;

            return Clone(withPlayers: nextPlayers, withCurrentPlayer: cp);
        }
    }
}
