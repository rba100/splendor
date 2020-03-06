﻿
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Core
{
    public static class Utility
    {
        /// <summary>
        /// Mutates a list, reordering the contents at random.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var r = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static Dictionary<T, T2> CreateCopy<T, T2>(this IReadOnlyDictionary<T, T2> dictionary)
        {
            return dictionary.ToDictionary(d => d.Key, d => d.Value);
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
