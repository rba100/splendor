using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor
{
    public class GameState
    {
        public GameState(IDictionary<CoinColour, int> coinsAvailable, NobleTier nobleTier, BoardTier[] tiers, Player[] players)
        {
            CoinsAvailable = coinsAvailable ?? throw new ArgumentNullException(nameof(coinsAvailable));
            NobleTier = nobleTier ?? throw new ArgumentNullException(nameof(nobleTier));
            Tiers = tiers?.ToArray() ?? throw new ArgumentNullException(nameof(tiers));
            Players = players?.ToArray() ?? throw new ArgumentNullException(nameof(players));
            CurrentPlayer = Players.First();
        }

        public IDictionary<CoinColour, int> CoinsAvailable { get; private set; }
        public NobleTier NobleTier { get; private set; }
        public BoardTier[] Tiers { get; private set; }
        public Player[] Players { get; private set; }
        public Player CurrentPlayer { get; set; }
    }
}
