
using System;
using System.Collections.Generic;

namespace Splendor.Core
{
    internal static class Utility
    {
        private static readonly Random s_Rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = s_Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IReadOnlyDictionary<CoinColour, int> CoinQuantity(int black = 0, int blue = 0, int red = 0, int green = 0, int white = 0, int gold = 0)
        {
            return new Dictionary<CoinColour, int>
            {
                { CoinColour.Black, black },
                { CoinColour.Blue,  blue  },
                { CoinColour.Red,   red   },
                { CoinColour.Green, green },
                { CoinColour.White, white },
                { CoinColour.Gold,  gold  },
            };
        }

        public static Dictionary<CoinColour, int> CreatePurse(int black = 0, int blue = 0, int red = 0, int green = 0, int white = 0, int gold = 0)
        {
            return new Dictionary<CoinColour, int>
            {
                { CoinColour.Black, black },
                { CoinColour.Blue,  blue  },
                { CoinColour.Red,   red   },
                { CoinColour.Green, green },
                { CoinColour.White, white },
                { CoinColour.Gold,  gold  },
            };
        }
    }
}
