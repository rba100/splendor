
using System.Collections.Generic;
using System.Linq;
using Splendor.Core;

using static System.Linq.Enumerable;

namespace Splendor.AIs.Unofficial
{
    internal static class ExtensionMethods
    {
        public static IPool ToPool(this IEnumerable<TokenColour> colours)
        {
            var pool = new Pool();

            foreach (var colour in colours)
            {
                pool[colour] += 1;
            }

            return pool;
        }

        public static IEnumerable<T[]> ItemPermutations<T>
            (this T[] collection, int indexCount)
        {
            return IndexPermutations(new int[] {}, indexCount, collection.Length)
                  .Select(l => l.Select(i => collection[i]).ToArray());
        }

        public static IEnumerable<int[]> IndexPermutations<T>
            (this IReadOnlyCollection<T> collection, int indexCount)
        {
            return IndexPermutations(new int[] {}, indexCount, collection.Count);
        }

        private static IEnumerable<int[]> IndexPermutations
            (int[] indexes, int indexCount, int collectionLength)
        {
            if (indexes.Length >= indexCount)
            {
                yield return indexes;

                yield break;
            }

            var startFromIndex = indexes.Any() ? indexes.Last() + 1 : 0;

            var permuteWith = Range(startFromIndex,
                                    collectionLength - startFromIndex);

            foreach (var index in permuteWith)
            {
                var results = IndexPermutations(indexes.Append(index).ToArray(),
                                                indexCount,
                                                collectionLength);

                foreach (var result in results) yield return result;
            }
        }
    }
}
