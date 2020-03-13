
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.AIs.Unofficial.Actions
{
    internal static class ExtensionMethods
    {
        /// <remarks>Fully evaluates the items enumeration once</remarks>
        ///
        /// <returns>Picks a random item using a uniform distribution</returns>
        ///
        public static T GetRandomItem<T>(this IEnumerable<T> items,
                                         Random random)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            if (random is null) throw new ArgumentNullException(nameof(random));

            var itemsArray = items.ToArray();

            return itemsArray[random.Next(itemsArray.Length)];
        }
    }
}