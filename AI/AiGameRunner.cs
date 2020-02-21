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
                var ai = _playerAi[index];
                var action = ai.ChooseAction(_engine.GameState);
                m_Log($"{_engine.GameState.CurrentPlayer.Name}, Action: {action}");
                if (action is NoAction) playersPassed++; else playersPassed = 0;
                action.Execute(_engine);
            }

            Console.WriteLine("Game over!");

            foreach (var player in _engine.GameState.Players)
            {
                var score = player.VictoryPoints();
                var s = score == 1 ? "" : "s";
                m_Log($"{player.Name} — {score} point{s}");
            }
        }
    }
}
