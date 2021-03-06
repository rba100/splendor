﻿using System;
using System.Collections.Generic;

namespace Splendor.Core
{
    /// <summary>
    /// When passed as IPool, class is treated as frozen by convention.
    /// </summary>
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

        /// <summary>
        /// Creates an empty pool.
        /// </summary>
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
            get
            {
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
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException();

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

        public Pool CreateCopy()
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

        /// <summary>
        /// Does not take into account gold-as-wildcard.
        /// </summary>
        public Pool DeficitFor(IPool other)
        {
            var gold = other.Gold - Gold; gold = gold < 0 ? 0 : gold;
            var white = other.White - White; white = white < 0 ? 0 : white;
            var blue = other.Blue - Blue; blue = blue < 0 ? 0 : blue;
            var red = other.Red - Red; red = red < 0 ? 0 : red;
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

        public IEnumerable<TokenColour> Colours(bool includeGold = true)
        {
            if (includeGold && Gold > 0) yield return TokenColour.Gold;
            if (White > 0) yield return TokenColour.White;
            if (Blue > 0) yield return TokenColour.Blue;
            if (Red > 0) yield return TokenColour.Red;
            if (Green > 0) yield return TokenColour.Green;
            if (Black > 0) yield return TokenColour.Black;
        }

        public int Sum => Gold + White + Blue + Red + Green + Black;
    }
}