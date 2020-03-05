using System;

namespace Splendor.Core.Domain
{
    public interface IPool
    {
        int this[TokenColour index] { get; }
        Pool Clone();
    }

    public sealed class Pool : IPool
    {
        public Pool(int gold, int white, int blue, int red, int green, int black)
        {
            Gold = gold;
            White = white;
            Blue = blue;
            Red = red;
            Green = green;
            Black = black;
        }

        public int Gold { get; private set; }
        public int White { get; private set; }
        public int Blue { get; private set; }
        public int Red { get; private set; }
        public int Green { get; private set; }
        public int Black { get; private set; }
        
        public int this[TokenColour index]    // Indexer declaration  
        {
            get {
                switch (index)
                {
                    case TokenColour.Gold:
                        return Gold;
                    case TokenColour.White:
                        return White;
                    case TokenColour.Red:
                        return Red;
                    case TokenColour.Blue:
                        return Blue;
                    case TokenColour.Green:
                        return Green;
                    case TokenColour.Black:
                        return Black;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            set {
                switch (index)
                {
                    case TokenColour.Gold:
                        Gold = value; break;
                    case TokenColour.White:
                        White = value; break;
                    case TokenColour.Red:
                        Red = value; break;
                    case TokenColour.Blue:
                        Blue = value; break;
                    case TokenColour.Green:
                        Green = value; break;
                    case TokenColour.Black:
                        Black = value; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Pool Clone()
        {
            return new Pool(Gold, White, Blue, Red, Green, Black);
        }
    }
}