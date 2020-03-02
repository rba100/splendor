using Splendor.Core;
using Splendor.Core.Actions;
using Splendor.Core.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor.ConsoleRunner
{
    public class InteractiveRunner
    {
        public void Run()
        {
            Console.Write("Enter name: ");
            string playerName = Console.ReadLine();

            var ais = new ISpendorAi[] { new StupidSplendorAi("Skynet"), new StupidSplendorAi("Wopr") };
            var names = new[] { playerName }.Concat(ais.Select(a => a.Name)).ToArray();
            var gameState = new DefaultGameInitialiser(new DefaultCards()).Create(names);
            var game = new Game(gameState);
            while (!game.IsGameFinished)
            {
                IAction action = null;
                var turnPlayer = game.State.CurrentPlayer;
                if(turnPlayer.Name == playerName)
                {
                    PrintState(game);
                    bool turnComplete = false;
                    while (!turnComplete)
                    {
                        Console.Write(">");
                        var input = Console.ReadLine();
                        try
                        {
                            action = GetActionFromInput(input, game.State);
                            if (action != null)
                            {
                                game.CommitTurn(action);
                                turnComplete = true;
                            }
                        }catch(Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }

                }
                else // AI
                {
                    var ai = ais.Single(a => a.Name == game.State.CurrentPlayer.Name);
                    action = ai.ChooseAction(game.State);
                    game.CommitTurn(action);
                }

                var updatedTurnPlayer = game.State.Players.Single(p => p.Name == turnPlayer.Name);

                Console.WriteLine($"{updatedTurnPlayer.Name} {updatedTurnPlayer.VictoryPoints()}pts, {action}");
            }
            Console.WriteLine("****************************************");
            Console.WriteLine(game.TopPlayer.Name + " wins!");
            Console.WriteLine("****************************************");
        }

        private string PrintTokenPool(IReadOnlyDictionary<TokenColour, int> tokenPool)
        {
            var readOuts = tokenPool.
                Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            return string.Join(", ", readOuts);
        }

        private string PrintTokenPoolShort(IReadOnlyDictionary<TokenColour, int> tokenPool)
        {
            var readOuts = tokenPool.Where(c => c.Value > 0).Select(kvp => new string(FromCoinColour(kvp.Key)[0],kvp.Value)).ToList();
            return string.Join(" ", readOuts);
        }

        private void PrintState(IGame game)
        {
            Console.Write("Nobles: ");
            Console.WriteLine(string.Join(",", game.State.Nobles.Select(n => n.Name).ToArray()));
            foreach(var tier in game.State.Tiers.OrderByDescending(t => t.Tier))
            {
                foreach(var slot in tier.ColumnSlots.OrderBy(s=>s.Key))
                {
                    var card = slot.Value;
                    var buyIndicator = card == null ? " " : BuyCard.CanAffordCard(game.State.CurrentPlayer, card) ? "*" : " ";
                    Console.WriteLine($"{tier.Tier}-{slot.Key}{buyIndicator}: " + slot.Value?.ToString() ?? "[EMPTY]");
                }
            }
            foreach (var card in game.State.CurrentPlayer.ReservedCards)
            {
                Console.WriteLine($"Res : " + card.ToString());
            }

            Console.WriteLine($"Bank: {PrintTokenPoolShort(game.State.TokensAvailable)}");
            var purseValues = game.State.CurrentPlayer.Purse.Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            Console.WriteLine("Purse: " + string.Join(", ", purseValues));
            var spendingPower = game.State.CurrentPlayer.Purse.MergeWith(game.State.CurrentPlayer.GetDiscount()).Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            Console.WriteLine("Can afford: " + string.Join(", ", spendingPower));
        }

        private IAction GetActionFromInput(string input, GameState state)
        {
            var i = input.ToLowerInvariant().Trim();

            if (i.StartsWith("take"))
            {
                var codes = i.Substring("take".Length);
                var tokens = TokenPoolFromInput(codes);
                return new TakeTokens(tokens, Utility.CreateEmptyTokenPool());
            };

            if (i.StartsWith("buy"))
            {
                var args = i.Substring("buy".Length).Trim();
                if (args.StartsWith("res"))
                {
                    var resCard = state.CurrentPlayer.ReservedCards.FirstOrDefault(c => BuyCard.CanAffordCard(state.CurrentPlayer, c));
                    if(resCard == null) throw new ArgumentException("You can't afford any of your reserved cards.");
                    var resPayment = BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, resCard);
                    if (resPayment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                    return new BuyCard(resCard, resPayment);
                }
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'buy 1-2' for buying the second card in tier one.");
                var tier = int.Parse(args[0].ToString());                
                var cardIndex = int.Parse(args[2].ToString());
                var card = state.Tiers.Single(t => t.Tier == tier).ColumnSlots[cardIndex];
                var payment = args.Length > 3 ? TokenPoolFromInput(args.Substring(3)) : BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, card);
                if (payment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                return new BuyCard(card, payment);
            }

            if (i.StartsWith("reserve"))
            {
                var args = i.Substring("reserve".Length).Trim();
                var tier = int.Parse(args[0].ToString());
                if (args.Length == 1) return new ReserveFaceDownCard(tier);
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'buy 1-2' for buying the second card in tier one.");                
                var cardIndex = int.Parse(args[2].ToString());
                var card = state.Tiers.Single(t => t.Tier == tier).ColumnSlots[cardIndex];
                return new ReserveCard(card);
            }

            throw new NotImplementedException("Don't know that one.");
        }

        private Dictionary<TokenColour, int> TokenPoolFromInput(string input)
        {
            var tokens = Utility.CreateEmptyTokenPool();
            foreach (char c in input.Trim().Replace(" ", ""))
            {
                switch (c)
                {
                    case 'r': tokens[TokenColour.Red] += 1; break;
                    case 'g': tokens[TokenColour.Green] += 1; break;
                    case 'u': tokens[TokenColour.Blue] += 1; break;
                    case 'b': tokens[TokenColour.Black] += 1; break;
                    case 'w': tokens[TokenColour.White] += 1; break;
                    default: throw new ArgumentException($"Unrecognised token symbol: '${c}'");
                }
            }
            return tokens;
        }
        private string FromCoinColour(TokenColour col)
        {
            switch (col)
            {
                case TokenColour.White: return "W";
                case TokenColour.Blue: return "U";
                case TokenColour.Red: return "R";
                case TokenColour.Green: return "G";
                case TokenColour.Black: return "B";
                case TokenColour.Gold: return "$";
                default: throw new ArgumentOutOfRangeException(nameof(col));
            }
        }
    }
}
