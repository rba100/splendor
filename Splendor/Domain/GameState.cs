
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;

namespace Splendor
{
    public class GameState
    {
        public GameState(IDictionary<TokenColour, int> coinsAvailable, ICollection<Noble> nobles, BoardTier[] tiers, Player[] players)
        {
            TokensAvailable = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            Nobles = nobles ?? throw new ArgumentNullException(nameof(nobles));
            Tiers = tiers?.ToArray() ?? throw new ArgumentNullException(nameof(tiers));
            Players = players?.ToArray() ?? throw new ArgumentNullException(nameof(players));
            CurrentPlayer = Players.First();
        }

        public IDictionary<TokenColour, int> TokensAvailable { get; private set; }
        public ICollection<Noble> Nobles { get; private set; }
        public BoardTier[] Tiers { get; private set; }
        public Player[] Players { get; private set; }
        public Player CurrentPlayer { get; set; }

        public GameState CopyWith(IDictionary<TokenColour, int> coinsAvailable = null, ICollection<Noble> nobles = null, BoardTier[] tiers = null, Player[] players = null)
        {
            return new GameState(
                coinsAvailable ?? TokensAvailable.CreateCopy(),
                nobles ?? Nobles,
                tiers ?? Tiers,
                players ?? Players);
        }
    }
}
