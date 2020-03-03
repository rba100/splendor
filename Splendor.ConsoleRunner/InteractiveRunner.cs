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
                    var buyIndicator = GetBuyIndicator(card, game.State.CurrentPlayer);
                    Console.WriteLine($"{tier.Tier}-{slot.Key}{buyIndicator}: " + slot.Value?.ToString() ?? "[EMPTY]");
                }
            }
            foreach (var card in game.State.CurrentPlayer.ReservedCards)
            {
                var buyIndicator = GetBuyIndicator(card, game.State.CurrentPlayer);
                Console.WriteLine($"Res{buyIndicator}: " + card.ToString());
            }

            Console.WriteLine($"Bank: {PrintTokenPoolShort(game.State.TokensAvailable)}");
            var purseValues = game.State.CurrentPlayer.Purse.Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            Console.WriteLine("Purse: " + string.Join(", ", purseValues));
            Console.WriteLine($"Bonuses: {PrintTokenPoolShort(game.State.CurrentPlayer.GetDiscount())}");
            var spendingPower = game.State.CurrentPlayer.Purse.MergeWith(game.State.CurrentPlayer.GetDiscount()).Where(c => c.Value > 0).Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
            Console.WriteLine("Can afford: " + string.Join(", ", spendingPower));
        }

        private string GetBuyIndicator(Card card, Player player)
        {
            var buyIndicator = card == null 
                ? " " 
                : BuyCard.CanAffordCard(player, card) 
                   ? "*" 
                   : player.GetDiscount().MergeWith(player.Purse).GetDeficitFor(card.Cost).SumValues() < 3 ? "·" : " ";
            return buyIndicator;
        }

        private IAction GetActionFromInput(string input, GameState state)
        {
            var i = input.ToLowerInvariant().Trim();

            if (i.StartsWith("h"))
            {
                Console.WriteLine("Commands: refer to cards buy their tier and index, e.g. '1-2' for tier 1, 2nd card.");
                Console.WriteLine("    t: take tokens, e.g. 't ugb' for take blUe, Green, Black.");
                Console.WriteLine("    b: buy card. e.g. 'b 1-2', or 'b res' to buy a reserved card (choice made for you).");
                Console.WriteLine("    r: reserve card. e.g. 'r 1-2', or just 'r 1' to reserve a random face down card from tier 1.");
                Console.WriteLine("Instruments:");
                Console.WriteLine("    *: you can afford this card.");
                Console.WriteLine("    ·: you can almost afford this card.");
                return null;
            }

            if (i.StartsWith("t"))
            {
                var whiteSpace = i.IndexOf(' ');
                var codes = i.Substring(whiteSpace);
                var tokens = TokenPoolFromInput(codes);
                return new TakeTokens(tokens, Utility.CreateEmptyTokenPool());
            };

            if (i.StartsWith("b"))
            {
                var whiteSpace = i.IndexOf(' ');
                var args = i.Substring(whiteSpace).Trim();
                if (args.StartsWith("res"))
                {
                    var resCard = state.CurrentPlayer.ReservedCards.OrderByDescending(c=>c.VictoryPoints).FirstOrDefault(c => BuyCard.CanAffordCard(state.CurrentPlayer, c));
                    if(resCard == null) throw new ArgumentException("You can't afford any of your reserved cards.");
                    var resPayment = BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, resCard);
                    if (resPayment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                    return new BuyCard(resCard, resPayment);
                }
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'b 1-2' for buying the second card in tier one.");
                var tier = int.Parse(args[0].ToString());                
                var cardIndex = int.Parse(args[2].ToString());
                var card = state.Tiers.Single(t => t.Tier == tier).ColumnSlots[cardIndex];
                var payment = args.Length > 3 ? TokenPoolFromInput(args.Substring(3)) : BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, card);
                if (payment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                return new BuyCard(card, payment);
            }

            if (i.StartsWith("r"))
            {
                var whiteSpace = i.IndexOf(' ');
                var args = i.Substring(whiteSpace).Trim();
                var tier = int.Parse(args[0].ToString());
                if (args.Length == 1) return new ReserveFaceDownCard(tier);
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'r 1-2' for reserving the second card in tier one.");                
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
