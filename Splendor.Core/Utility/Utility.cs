
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public static class Utility
    {
        private static readonly Random s_Rng = new Random();

        /// <summary>
        /// Mutates a list, reordering the contents at random.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
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

        public static Dictionary<TokenColour,int> CreateEmptyTokenPool()
        {
            return new Dictionary<TokenColour, int>
            {
                { TokenColour.Gold, 0 },
                { TokenColour.White, 0 },
                { TokenColour.Red, 0 },
                { TokenColour.Blue, 0 },
                { TokenColour.Green, 0 },
                { TokenColour.Black, 0 }
            };
        }

        public static Dictionary<T, T2> CreateCopy<T, T2>(this IReadOnlyDictionary<T, T2> dictionary)
        {
            return dictionary.ToDictionary(d => d.Key, d => d.Value);
        }

        public static Dictionary<TokenColour, int> MergeWith(this IReadOnlyDictionary<TokenColour, int> dictionary, IReadOnlyDictionary<TokenColour, int> other)
        {
            return new Dictionary<TokenColour, int>
            {
                { TokenColour.Gold, dictionary[TokenColour.Gold] + other[TokenColour.Gold] },
                { TokenColour.White, dictionary[TokenColour.White] + other[TokenColour.White] },
                { TokenColour.Red, dictionary[TokenColour.Red] + other[TokenColour.Red] },
                { TokenColour.Blue, dictionary[TokenColour.Blue] + other[TokenColour.Blue] },
                { TokenColour.Green, dictionary[TokenColour.Green] + other[TokenColour.Green] },
                { TokenColour.Black, dictionary[TokenColour.Black] + other[TokenColour.Black] }
            };
        }

        public static Dictionary<TokenColour, int> MergeWith(this IReadOnlyDictionary<TokenColour, int> dictionary, IPool other)
        {
            return new Dictionary<TokenColour, int>
            {
                { TokenColour.Gold, dictionary[TokenColour.Gold] + other[TokenColour.Gold] },
                { TokenColour.White, dictionary[TokenColour.White] + other[TokenColour.White] },
                { TokenColour.Red, dictionary[TokenColour.Red] + other[TokenColour.Red] },
                { TokenColour.Blue, dictionary[TokenColour.Blue] + other[TokenColour.Blue] },
                { TokenColour.Green, dictionary[TokenColour.Green] + other[TokenColour.Green] },
                { TokenColour.Black, dictionary[TokenColour.Black] + other[TokenColour.Black] }
            };
        }

        public static Dictionary<TokenColour, int> GetDeficitFor(this IReadOnlyDictionary<TokenColour, int> dictionary, IReadOnlyDictionary<TokenColour, int> other)
        {
            return new Dictionary<TokenColour, int>
            {
                { TokenColour.Gold,  Math.Max(0, other[TokenColour.Gold]  - dictionary[TokenColour.Gold] ) },
                { TokenColour.White, Math.Max(0, other[TokenColour.White] - dictionary[TokenColour.White]) },
                { TokenColour.Red,   Math.Max(0, other[TokenColour.Red]   - dictionary[TokenColour.Red]  ) },
                { TokenColour.Blue,  Math.Max(0, other[TokenColour.Blue]  - dictionary[TokenColour.Blue] ) },
                { TokenColour.Green, Math.Max(0, other[TokenColour.Green] - dictionary[TokenColour.Green]) },
                { TokenColour.Black, Math.Max(0, other[TokenColour.Black] - dictionary[TokenColour.Black]) }
            };
        }

        public static Dictionary<TokenColour, int> GetDeficitFor(this IReadOnlyDictionary<TokenColour, int> dictionary, IPool other)
        {
            return new Dictionary<TokenColour, int>
            {
                { TokenColour.Gold,  Math.Max(0, other[TokenColour.Gold]  - dictionary[TokenColour.Gold] ) },
                { TokenColour.White, Math.Max(0, other[TokenColour.White] - dictionary[TokenColour.White]) },
                { TokenColour.Red,   Math.Max(0, other[TokenColour.Red]   - dictionary[TokenColour.Red]  ) },
                { TokenColour.Blue,  Math.Max(0, other[TokenColour.Blue]  - dictionary[TokenColour.Blue] ) },
                { TokenColour.Green, Math.Max(0, other[TokenColour.Green] - dictionary[TokenColour.Green]) },
                { TokenColour.Black, Math.Max(0, other[TokenColour.Black] - dictionary[TokenColour.Black]) }
            };
        }

        public static int SumValues(this IReadOnlyDictionary<TokenColour, int> dictionary)
        {
            return dictionary.Values.Sum();
        }

        public static TokenColour[] NonZeroColours(this IReadOnlyDictionary<TokenColour, int> dictionary)
        {
            return dictionary.Where(kvp=>kvp.Value > 0).Select(kvp=>kvp.Key).ToArray();
        }

        public static IEnumerable<TokenColour[]> GetAllThreeColourCombinations(IEnumerable<TokenColour> colours)
        {
            var clrs = colours.ToArray();
            if (clrs.Length < 3) yield break;
            for (int i = 0; i < clrs.Length - 2; i++)
            {
                for (int j = i + 1; j < clrs.Length - 1; j++)
                {
                    for (int k = j + 1; k < clrs.Length; k++)
                    {
                        yield return new TokenColour[] { clrs[i], clrs[j], clrs[k] };
                    }
                }
            }
        }
    }
}
