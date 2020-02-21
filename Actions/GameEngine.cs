using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core.Actions
{
    /// <summary>
    /// Logic for game rules other than initialisation
    /// </summary>
    public class GameEngine : IGameEngine
    {
        public GameState GameState { get; }
        public bool IsGameFinished { get; private set; }

        public GameEngine(GameState game)
        {
            GameState = game ?? throw new ArgumentNullException(nameof(game));
        }

        public IEnumerable<IAction> GetPossibleActions()
        {
            yield break;
        }

        public void CommitTurn()
        {
            IsGameFinished = 
                GameState.Players.Last() == GameState.CurrentPlayer
                && GameState.Players.Any(p => p.VictoryPoints() >= 15);

            var nextIndex = (Array.IndexOf(GameState.Players, GameState.CurrentPlayer) + 1)
                % GameState.Players.Length;
            GameState.CurrentPlayer = GameState.Players[nextIndex];
        }
    }

    public interface IGameEngine
    {
        GameState GameState { get; }
        void CommitTurn();
        IEnumerable<IAction> GetPossibleActions();
    }

    public interface IAction
    {
        void Execute(GameEngine gameEngine);
    }

    public class NoAction : IAction
    {
        public void Execute(GameEngine gameEngine)
        {
            gameEngine.CommitTurn();
        }
    }

    public class BuyCard : IAction
    {
        public Player Player { get; }
        public Card Card { get; }
        public IReadOnlyDictionary<CoinColour, int> CoinsTaken { get; }

        public BuyCard(Player player, Card card, IReadOnlyDictionary<CoinColour, int> coinsTaken)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Card = card ?? throw new ArgumentNullException(nameof(card));
            CoinsTaken = coinsTaken ?? throw new ArgumentNullException(nameof(coinsTaken));
        }

        public void Execute(GameEngine gameEngine)
        {

            gameEngine.CommitTurn();
        }
    }
}
