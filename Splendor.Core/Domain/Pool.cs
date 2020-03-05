using System;

namespace Splendor.Core
{
    public interface IPool
    {
        int this[TokenColour index] { get; }
        Pool Clone();
        Pool DeficitFor(IPool other);
        Pool MergeWith(IPool other);
        int Gold { get;}
        int White { get; }
        int Blue { get; }
        int Red { get; }
        int Green { get; }
        int Black { get; }
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

        public Pool()
        {
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

        public Pool MergeWith(IPool other)
        {
            return new Pool(
                Gold + other.Gold,
                White + other.White,
                Blue + other.Blue,
                Red + other.Red,
                Green + other.Green,
                Black + other.Black);
        }

        public Pool DeficitFor(IPool other)
        {
            var gold  = other.Gold  - Gold;  gold  = gold  < 0 ? 0 : gold;
            var white = other.White - White; white = white < 0 ? 0 : white;
            var blue  = other.Blue  - Blue;  blue  = blue  < 0 ? 0 : blue;
            var red   = other.Red   - Red;   red   = red   < 0 ? 0 : red;
            var green = other.Green - Green; green = green < 0 ? 0 : green;
            var black = other.Black - Black; black = black < 0 ? 0 : black;

            return new Pool(
                gold,
                white,
                blue,
                red,
                green,
                black);
        }
    }
}