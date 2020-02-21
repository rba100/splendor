using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor.Core.AI
{
    public class AiGameRunner
    {
        private readonly ISpendorAi[] _playerAi;
        private readonly IGameEngine _engine;

        private readonly Action<string> m_Log;

        public AiGameRunner(IEnumerable<ISpendorAi> players, Action<string> log)
        {
            _playerAi = players?.ToArray() ?? throw new ArgumentNullException(nameof(players));
            var state = new DefaultGameInitialiser(new DefaultCards()).Create(players: _playerAi.Length);
            _engine = new GameEngine(state);
            m_Log = log ?? new Action<string>(s => { });
        }

        public void Run()
        {
            int playersPassed = 0;
            while (!_engine.IsGameFinished && playersPassed < _engine.GameState.Players.Length)
            {
                var index = Array.IndexOf(_engine.GameState.Players, _engine.GameState.CurrentPlayer);
                var thisPlayer = _engine.GameState.Players[index];
                var ai = _playerAi[index];
                var action = ai.ChooseAction(_engine.GameState);
                if (action is NoAction) playersPassed++; else playersPassed = 0;
                action.Execute(_engine);
                m_Log($"{ai.Name} (Bank:{thisPlayer.Purse.Values.Sum()}), {action}");
            }

            Console.WriteLine($"**** Game over after {_engine.RoundsCompleted} rounds.");

            for (int i = 0; i < _engine.GameState.Players.Length; i++)
            {
                Player player = _engine.GameState.Players[i];
                var score = player.VictoryPoints();
                var s = score == 1 ? "" : "s";
                var nobles = player.Nobles.Count();
                var ns = nobles == 1 ? "" : "s";
                var nobleNames = nobles > 0 ? ": " + string.Join(", ", player.Nobles.Select(n => n.Name)) : "";
                var bonuses = string.Join(", ", player.GetDiscount().Where(kvp => kvp.Key != CoinColour.Gold)
                                                                    .Select(kvp=> $"{kvp.Value} {kvp.Key}"));
                m_Log($"{_playerAi[i].Name} — {score} point{s} ({nobles} noble{ns}{nobleNames}) (Bonuses {bonuses})");
            }
        }
    }
}
