using Splendor.Core;
using Splendor.Core.Actions;
using Splendor.Core.AI;
using System;
using System.Linq;

namespace Splendor.ConsoleGame
{
    public class InteractiveRunner
    {        
        public void Run()
        {
            ConsoleDrawing.InitFullScreenApp();
            ClearAndDrawFrame();
            string playerName = Query("What is your name?");

            var ais = new ISpendorAi[] { new StupidSplendorAi("Skynet"), new StupidSplendorAi("Wopr") };
            var names = new[] { playerName }.Concat(ais.Select(a => a.Name)).ToArray();
            var gameState = new DefaultGameInitialiser(new DefaultCards()).Create(names);
            var game = new Game(gameState);
            while (!game.IsGameFinished) // Process a turn in each iteration
            {
                var turnPlayer = game.State.CurrentPlayer;
                var isPlayerTurn = turnPlayer.Name == playerName;
                IAction action = null;
                if (isPlayerTurn)
                {
                    bool turnComplete = false;
                    string prompt = "What do you want to do?";
                    var promptColour = ConsoleColor.Yellow;
                    while (!turnComplete)
                    {
                        ClearAndDrawFrame();
                        PrintState(game);
                        var input = Query(prompt, promptColour);
                        if(input.ToLowerInvariant() == "quit") return;
                        try
                        {
                            action = GetActionFromInput(input, game.State);
                            if (action != null)
                            {
                                game.CommitTurn(action);
                                turnComplete = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            promptColour = ConsoleColor.Red;
                            prompt = ex.Message;
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
                //Console.WriteLine($"{updatedTurnPlayer.Name}, {action}");
            }
            PrintState(game);
            Query(game.TopPlayer.Name + " wins! Press enter to exit.", ConsoleColor.Red);
        }

        private void ClearAndDrawFrame()
        {
            ConsoleDrawing.Clear();
            var width = Console.WindowWidth;
            var height = Console.WindowHeight;
            ConsoleDrawing.DrawBoxDouble(0, 0, width, height);
            ConsoleDrawing.WriteAt('╠', 0, height - 3);
            ConsoleDrawing.WriteAt('╠', 0, height - 5);
            ConsoleDrawing.WriteAt('╣', width - 1, height - 3);
            ConsoleDrawing.WriteAt('╣', width - 1, height - 5);
            ConsoleDrawing.DrawLineX('═', 1, height - 3, width-2);
            ConsoleDrawing.DrawLineX('═', 1, height - 5, width-2);
        }

        private string Query(string prompt, ConsoleColor promptColour = ConsoleColor.Yellow)
        {
            SetPrompt(prompt, promptColour);
            ConsoleDrawing.BlankRegion(2, Console.WindowHeight - 2, Console.WindowWidth - 3, 1);
            Console.SetCursorPosition(2, Console.WindowHeight - 2);
            Console.Write(">");
            var result = Console.ReadLine();
            return result;
        }

        private static void SetPrompt(string prompt, ConsoleColor promptColour = ConsoleColor.Yellow)
        {
            ConsoleDrawing.BlankRegion(2, Console.WindowHeight - 4, Console.WindowWidth - 3, 1);
            Console.SetCursorPosition(2, Console.WindowHeight - 4);
            using (new ConsoleColour(promptColour)) Console.Write(prompt);
        }

        private string QueryBox(string prompt)
        {
            var width = prompt.Length + 4;
            var height = 4;
            var offsetX = (Console.WindowWidth - width) / 2;
            var offSetY = (Console.WindowHeight - height) / 2;

            ConsoleDrawing.DrawBoxDouble(offsetX, offSetY, width, height);
            Console.SetCursorPosition(offsetX + 2, offSetY + 1);
            Console.Write(prompt);
            Console.SetCursorPosition(offsetX + 2, offSetY + 2);
            return Console.ReadLine();
        }


        private void PrintState(IGame game)
        {
            var marginLeft = Math.Max(0, (Console.WindowWidth / 2) - 26);

            Console.CursorTop = 2;
            // Print players
            foreach (var p in game.State.Players)
            {
                PrintPlayerTerse(p);
                Console.WriteLine();
            }

            Console.CursorTop = 4;

            var player = game.State.CurrentPlayer;
            var budget = player.Budget;
            Console.CursorLeft = marginLeft;
            Console.Write("Nobles: ");
            Console.WriteLine(string.Join(",", game.State.Nobles.Select(n => n.Name).ToArray()));
            foreach (var tier in game.State.Tiers.OrderByDescending(t => t.Tier))
            {
                foreach (var slot in tier.ColumnSlots.OrderBy(s => s.Key))
                {
                    var card = slot.Value;
                    var buyIndicator = GetBuyIndicator(card, player);
                    Console.CursorLeft = marginLeft;
                    Console.Write($"{tier.Tier}-{slot.Key}{buyIndicator}: ");
                    PrintCardLine(card, budget);
                }
            }
            foreach (var card in game.State.CurrentPlayer.ReservedCards)
            {
                var buyIndicator = GetBuyIndicator(card, player);
                Console.CursorLeft = marginLeft;
                Console.Write($"Res{buyIndicator}: ");
                PrintCardLine(card, budget);
            }

            Console.CursorLeft = marginLeft; Console.Write("Bank: "); PrintTokenPoolShortWithColours(game.State.Bank); Console.WriteLine();
            var purseValues = player.Purse.Colours().Select(col => $"{player.Purse[col]} {col}").ToList();
            Console.CursorLeft = marginLeft; Console.WriteLine($"Purse ({game.State.CurrentPlayer.Purse.Sum}): " + string.Join(", ", purseValues));
            Console.CursorLeft = marginLeft; Console.Write("Bonuses: "); PrintTokenPoolShortWithColours(game.State.CurrentPlayer.Bonuses); Console.WriteLine();
            Console.CursorLeft = marginLeft; Console.Write("Can afford: "); PrintTokenPoolShortWithColoursAsNumers(game.State.CurrentPlayer.Budget); Console.WriteLine();
        }

        private void PrintTokenPoolShortWithColours(IPool tokenPool, string separator = " ")
        {
            foreach (var col in tokenPool.Colours())
            {
                var str = new string(FromCoinColour(col)[0], tokenPool[col]);
                Write(str + separator, ToConsole(col));
            }
        }

        private void PrintPlayerTerse(Player p)
        {
            var noblesTerse = string.Join(", ", p.Nobles.Select(n => n.Name).ToArray());
            var s = p.Nobles.Count == 1 ? "" : "s";
            if (noblesTerse != string.Empty) noblesTerse = $", noble{s} {noblesTerse}";
            ConsoleDrawing.WriteAt($"{p.Name} ({p.VictoryPoints}){noblesTerse}", 2, Console.CursorTop);
            Console.WriteLine();
            Console.SetCursorPosition(2, Console.CursorTop);
            
            PrintTokenPoolShortWithColours(p.Bonuses, ""); Console.WriteLine();
            Console.SetCursorPosition(2, Console.CursorTop);
            PrintTokenPoolShortWithColoursAsNumers(p.Purse, ""); Console.WriteLine();
        }

        private void PrintTokenPoolShortWithColoursAsNumers(IPool tokenPool, string separator = " ")
        {
            foreach (var col in tokenPool.Colours())
            {
                if (tokenPool[col] == 0) continue;
                Write(tokenPool[col] + separator, ToConsole(col));
            }
        }

        private char GetBuyIndicator(Card card, Player player)
        {
            if (card == null) return ' ';

            // '*' can afford
            // '·' can almost afford
            // ' ' simply cannot afford

            var buyIndicator = BuyCard.CanAffordCard(player, card)
                   ? '*'
                   : player.Budget.DeficitFor(card.Cost).Sum <= 3 ? '·' : ' ';

            return buyIndicator;
        }

        private IAction GetActionFromInput(string input, GameState state)
        {
            var i = input.ToLowerInvariant().Trim();

            if (i.StartsWith("h"))
            {
                var messages = new string[]
                    { "Commands: refer to cards buy their tier and index, e.g. '1-2' for tier 1, 2nd card."
                    ,"    t: take tokens, e.g. 't ugb' for take blUe, Green, Black."
                    ,"    b: buy card. e.g. 'b 1-2', or 'b res' to buy a reserved card (choice made for you if you have more than one)."
                    ,"    r: reserve card. e.g. 'r 1-2', or just 'r 1' to reserve a random face down card from tier 1."
                    ,"Instruments:"
                    ,"    *: you can afford this card."
                    ,"    ·: you can almost afford this card." };

                ConsoleDrawing.MessageBox(messages);
                Console.ReadKey();
                return null;
            }

            if (i.StartsWith("t"))
            {
                var codes = i.Substring(1);
                var tokens = TokenPoolFromInput(codes);
                IPool tokensToReturn = new Pool();
                var commaIndex = codes.IndexOf(',');
                if (commaIndex != -1)
                {
                    tokensToReturn = TokenPoolFromInput(codes.Substring(commaIndex + 1));
                }
                return new TakeTokens(tokens, tokensToReturn);
            };

            if (i.StartsWith("b"))
            {
                var args = i.Substring(1).Trim();
                if (args.StartsWith("res"))
                {
                    var resCard = state.CurrentPlayer.ReservedCards.OrderByDescending(c => c.VictoryPoints).FirstOrDefault(c => BuyCard.CanAffordCard(state.CurrentPlayer, c));
                    if (resCard == null) throw new ArgumentException("You can't afford any of your reserved cards.");
                    var resPayment = BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, resCard);
                    if (resPayment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                    return new BuyCard(resCard, resPayment);
                }
                if (args[1] != '-') throw new ArgumentException($"Syntax error. Usage: 'b 1-2' for buying the second card in tier one.");
                var tier = int.Parse(args[0].ToString());
                var cardIndex = int.Parse(args[2].ToString());
                var card = state.Tiers.Single(t => t.Tier == tier).ColumnSlots[cardIndex];
                if (card == null) throw new ArgumentException("There's no card in that slot. The tier {tier} deck has been exhasted.");
                var payment = args.Length > 3 ? TokenPoolFromInput(args.Substring(3)) : BuyCard.CreateDefaultPaymentOrNull(state.CurrentPlayer, card);
                if (payment == null) throw new ArgumentException("You cannot afford this card or did not specify sufficient payment.");
                return new BuyCard(card, payment);
            }

            if (i.StartsWith("r"))
            {
                var args = i.Substring(1).Trim();
                var tier = int.Parse(args[0].ToString());
                IPool tokensToReturn = new Pool();
                var commaIndex = args.IndexOf(',');
                if (commaIndex != -1)
                {
                    tokensToReturn = TokenPoolFromInput(args.Substring(commaIndex + 1));
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
            foreach (var colour in card.Cost.Colours())
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
