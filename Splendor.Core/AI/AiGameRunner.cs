using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor.Core.AI
{
    public class AiGameRunner
    {
        private readonly ICollection<ISpendorAi> _players;
        private readonly IGame _game;

        private readonly Action<string> m_Log;

        public AiGameRunner(IEnumerable<ISpendorAi> players, Action<string> log)
        {
            _players = players.ToArray();
            var state = new DefaultGameInitialiser(new DefaultCards()).Create(_players.Select(a => a.Name));
            _game = new Game(state);
            m_Log = log ?? new Action<string>(s => { });
        }

        public Dictionary<ISpendorAi, int> Run()
        {
            int playersPassed = 0;
            while (!_game.IsGameFinished && playersPassed < _game.State.Players.Count)
            {
                var turnPlayer = _game.State.CurrentPlayer;
                var turnAi = _players.Single(p => p.Name == turnPlayer.Name);
                var action = turnAi.ChooseAction(_game.State);
                if (action is NoAction) playersPassed++; else playersPassed = 0;
                _game.CommitTurn(action);
                m_Log($"{turnAi.Name} (Bank:{turnPlayer.Purse.Values.Sum()}), {action}");
            }

            m_Log($"**** Game over after {_game.RoundsCompleted} rounds. Winner: " + _game.TopPlayer.Name);

            var results = new Dictionary<ISpendorAi, int>();
            foreach (var player in _game.State.Players)
            {
                var ai = _players.Single(p => p.Name == player.Name);
                var score = player.VictoryPoints;
                var s = score == 1 ? "" : "s";
                var nobles = player.Nobles.Count();
                var ns = nobles == 1 ? "" : "s";
                var nobleNames = nobles > 0 ? ": " + string.Join(", ", player.Nobles.Select(n => n.Name)) : "";
                var bonuses = string.Join(", ", player.Bonuses.Where(kvp => kvp.Key != TokenColour.Gold)
                                                                    .Select(kvp=> $"{kvp.Value} {kvp.Key}"));
                m_Log($"{ai.Name} — {score} point{s} ({nobles} noble{ns}{nobleNames}) (Bonuses {bonuses})");
                results.Add(ai, score);
            }
            return results;
        }
    }
}
