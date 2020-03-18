
using System.Linq;

using NUnit.Framework;

namespace Splendor.AIs.Unofficial.Tests
{
    internal sealed class ExtensionMethodsTests
    {
        [Test]
        public void ItemPermutations_returns_correct_results()
        {
            var permutations = new [] { 1, 3, 6, 9, 9 }.ItemPermutations(3)
                                                       .ToArray();

            Assert.That(permutations, Has.Length.EqualTo(10));

            Assert.That(permutations, Has.Exactly(1).EqualTo(new [] { 1, 3, 6 }));
            Assert.That(permutations, Has.Exactly(1).EqualTo(new [] { 6, 9, 9 }));

            Assert.That(permutations, Has.Exactly(2).EqualTo(new [] { 1, 3, 9 }));
            Assert.That(permutations, Has.Exactly(1).EqualTo(new [] { 1, 9, 9 }));
            Assert.That(permutations, Has.Exactly(2).EqualTo(new [] { 3, 6, 9 }));

            Assert.That(permutations, Has.No.EqualTo(new [] { 3, 1, 6 }));
            Assert.That(permutations, Has.No.EqualTo(new [] { 9, 9, 6 }));
        }
    }
}
