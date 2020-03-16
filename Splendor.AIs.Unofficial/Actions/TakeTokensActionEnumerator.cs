
using System;
using System.Collections.Generic;
using System.Linq;

using Splendor.Core;
using Splendor.Core.Actions;

using static System.Linq.Enumerable;

namespace Splendor.AIs.Unofficial.Actions
{
    public sealed class TakeTokensActionEnumerator : IActionEnumerator
    {
        public IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState)
        {
            if (gameState is null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            if (gameState.Bank.Sum <= 0) yield break;

            var actionVariations = TakeTwoVariations(gameState)
                                  .Concat(TakeThreeVariations(gameState));

            foreach (var group in actionVariations) yield return group;
        }

        private static IEnumerable<IEnumerable<TakeTokens>> TakeTwoVariations
            (GameState gameState)
        {
            var colours = gameState.Bank.Colours(includeGold: false)
                                        .Where(c => gameState.Bank[c] >= 4);

            foreach (var colour in colours)
            {
                yield return WithReturnVariations(new Pool { [colour] = 2 },
                                                  gameState);
            }
        }

        private static IEnumerable<IEnumerable<TakeTokens>> TakeThreeVariations
            (GameState gameState)
        {
            var colours = gameState.Bank.Colours(includeGold: false).ToArray();

            var indexPermutations = IndexPermutations
                (new int[] {}, 3, colours.Length);

            var toTakePermutations = indexPermutations
                .Select(l => l.Select(i => colours[i]))
                .Select(ToPool);

            foreach (var toTake in toTakePermutations)
            {
                yield return WithReturnVariations(toTake, gameState);
            }
        }

        private static IEnumerable<TakeTokens> WithReturnVariations
            (IPool toTake, GameState gameState)
        {
            TakeTokens Take(IPool toReturn) => new TakeTokens(toTake, toReturn);

            var totalTokens = gameState.CurrentPlayer.Purse.MergeWith(toTake);

            var returnCount = totalTokens.Sum - 10;

            if (returnCount <= 0) return new [] { Take(new Pool()) };

            var allTokens = totalTokens
                .Colours()
                .SelectMany(c => Repeat(c, Math.Min(totalTokens[c], returnCount)))
                .ToArray();

            var indexPermutations = IndexPermutations
                (new int[] {}, returnCount, allTokens.Length);

            return indexPermutations
                  .Select(l => l.Select(i => allTokens[i]).ToArray())
                  .Select(l => { Array.Sort(l); return l; })
                  .Distinct(new ColoursEqualityComparer())
                  .Select(ToPool)
                  .Select(Take);
        }

        private static IEnumerable<int[]> IndexPermutations
            (int[] indexes, int returnCount, int allTokensLength)
        {
            if (indexes.Length >= returnCount)
            {
                yield return indexes;

                yield break;
            }

            var startFromIndex = indexes.Any() ? indexes.Last() + 1 : 0;

            var permuteWith = Range(startFromIndex,
                                    allTokensLength - startFromIndex);

            foreach (var index in permuteWith)
            {
                var results = IndexPermutations(indexes.Append(index).ToArray(),
                                                returnCount,
                                                allTokensLength);

                foreach (var result in results) yield return result;
            }
        }

        private static IPool ToPool(IEnumerable<TokenColour> colours)
        {
            var pool = new Pool();

            foreach (var colour in colours)
            {
                pool[colour] += 1;
            }

            return pool;
        }

        private sealed class ColoursEqualityComparer
            :
            IEqualityComparer<TokenColour[]>
        {
            public bool Equals(TokenColour[] x, TokenColour[] y) =>
                x.SequenceEqual(y);

            public int GetHashCode(TokenColour[] obj) =>
                unchecked(obj.Aggregate(17, (a, b) => 23 * a + (int)b));
        }
    }
}