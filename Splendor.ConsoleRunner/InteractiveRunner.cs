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

                Console.WriteLine($"{updatedTurnPlayer.Name} {updatedTurnPlayer.VictoryPoints}pts, {action}");
            }
            Console.WriteLine("****************************************");
            Console.WriteLine(game.TopPlayer.Name + " wins!");
            Console.WriteLine("****************************************");
        }

        private void PrintState(IGame game)
        {
            var player = game.State.CurrentPlayer;
            var budget = player.Budget;
            Console.Write("Nobles: ");
            Console.WriteLine(string.Join(",", game.State.Nobles.Select(n => n.Name).ToArray()));
            foreach(var tier in game.State.Tiers.OrderByDescending(t => t.Tier))
            {
                foreach(var slot in tier.ColumnSlots.OrderBy(s=>s.Key))
                {
                    var card = slot.Value;
                    var buyIndicator = GetBuyIndicator(card, budget);
                    Console.Write($"{tier.Tier}-{slot.Key}{buyIndicator}: ");
                    PrintCardLine(card, budget);
                }
            }
            foreach (var card in game.State.CurrentPlayer.ReservedCards)
            {
                var buyIndicator = GetBuyIndicator(card, budget);
                Console.Write($"Res{buyIndicator}: ");
                PrintCardLine(card, budget);
            }

            Console.Write("Bank: "); PrintTokenPoolShortWithColours(game.State.Bank); Console.WriteLine();
            var purseValues = player.Purse.Colours().Select(col => $"{player.Purse[col]} {col}").ToList();
            Console.WriteLine($"Purse ({game.State.CurrentPlayer.Purse.Sum}): " + string.Join(", ", purseValues));
            Console.Write("Bonuses: "); PrintTokenPoolShortWithColours(game.State.CurrentPlayer.Bonuses); Console.WriteLine();
            Console.Write("Can afford: "); PrintTokenPoolShortWithColoursAsNumers(game.State.CurrentPlayer.Budget); Console.WriteLine();
        }

        private void PrintTokenPoolShortWithColours(IPool tokenPool)
        {
            foreach (var col in tokenPool.Colours())
            {
                var str = new string(FromCoinColour(col)[0], tokenPool[col]);
                Write(str + " ", ToConsole(col));
            }
        }

        private void PrintTokenPoolShortWithColoursAsNumers(IPool tokenPool)
        {
            foreach (var col in tokenPool.Colours())
            {
                if (tokenPool[col] == 0) continue;
                Write(tokenPool[col] + " ", ToConsole(col));
            }
        }

        private char GetBuyIndicator(Card card, IPool budget)
        {
            if (card == null) return ' ';

            // '*' can afford
            // '·' can almost afford
            // ' ' simply cannot afford

            var buyIndicator = BuyCard.CanAffordCard(budget, card)
                   ? '*'
                   : budget.DeficitFor(card.Cost).Sum <= 3 ? '·' : ' ';

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
                IPool tokensToReturn = new Pool();
                var commaIndex = codes.IndexOf(',');
                if (commaIndex != -1)
                {
                    tokensToReturn = TokenPoolFromInput(codes.Substring(commaIndex+1));
                }
                return new TakeTokens(tokens, tokensToReturn);
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
                if(card == null) throw new ArgumentException("There's no card in that slot. The tier {tier} deck has been exhasted.");
                var payment = args.Length > 3 ? TokenPoolFromInput(args.Substring(3)) : BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, card);
                if (payment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                return new BuyCard(card, payment);
            }

            if (i.StartsWith("r"))
            {
                var whiteSpace = i.IndexOf(' ');
                var args = i.Substring(whiteSpace).Trim();
                var tier = int.Parse(args[0].ToString());
                IPool tokensToReturn = new Pool();
                var commaIndex = args.IndexOf(',');
                if (commaIndex != -1)
                {
                    tokensToReturn = TokenPoolFromInput(args.Substring(commaIndex+1));
                }
                var colourToReturn = tokensToReturn.Colours().Cast<TokenColour?>().SingleOrDefault();
                if (args.Length == 1) return new ReserveFaceDownCard(tier, colourToReturn);
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'r 1-2' for reserving the second card in tier one.");                
                var cardIndex = int.Parse(args[2].ToString());               
                var card = state.Tiers.Single(t => t.Tier == tier).ColumnSlots[cardIndex];
                return new ReserveCard(card, colourToReturn);
            }

            throw new NotImplementedException("Don't know that one.");
        }

        private IPool TokenPoolFromInput(string input)
        {
            var tokens = new Pool();
            foreach (char c in input.Trim().Replace(" ", ""))
            {
                switch (c)
                {
                    case 'r': tokens[TokenColour.Red] += 1; break;
                    case 'g': tokens[TokenColour.Green] += 1; break;
                    case 'u': tokens[TokenColour.Blue] += 1; break;
                    case 'b': tokens[TokenColour.Black] += 1; break;
                    case 'w': tokens[TokenColour.White] += 1; break;
                    case ',': return tokens;
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

        private void Write(string message, ConsoleColor colour)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.Write(message);
            Console.ForegroundColor = old;
        }

        public void PrintCardLine(Card card, IPool budget)
        {
            if (card == null)
            {
                Console.WriteLine("[Empty]");
                return;
            }
            var padding = new string(' ', 8 - card.BonusGiven.ToString().Length - card.Tier);
            var tierMarker = new string('·', card.Tier);
            Console.Write($"{card.VictoryPoints}pt {card.BonusGiven}{tierMarker}{padding}");
            bool first = true;
            foreach(var colour in card.Cost.Colours())
            {
                var value = card.Cost[colour];
                if (!first) Console.Write(", ");
                first = false;
                var canAfford = budget[colour] >= value;
                var displayCol = canAfford ? ConsoleColor.White : ConsoleColor.Gray;
                Write($"{value} {colour}", displayCol);
            }
            Console.WriteLine();
        }

        private ConsoleColor ToConsole(TokenColour colour)
        {
            switch (colour)
            {
                case TokenColour.White:
                    return ConsoleColor.White;
                case TokenColour.Blue:
                    return ConsoleColor.Blue;
                case TokenColour.Red:
                    return ConsoleColor.Red;
                case TokenColour.Green:
                    return ConsoleColor.Green;
                case TokenColour.Black:
                    return ConsoleColor.DarkGray;
                case TokenColour.Gold:
                    return ConsoleColor.DarkYellow;
                default: throw new ArgumentOutOfRangeException(nameof(colour));
            }
        }
    }
}
