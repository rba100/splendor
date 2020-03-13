
using System.Collections.Generic;

using Splendor.Core;
using Splendor.Core.Actions;

namespace Splendor.AIs.Unofficial.Actions
{
    /// <remarks>Thread safe</remarks>
    ///
    public interface IActionEnumerator
    {
        /// <summary>
        /// Returns some distinct actions that the current player
        /// could take that do not violate any of the game's rules
        /// </summary>
        ///
        /// <remarks>
        /// Never returns null, never contains a null enumeration or action.
        /// Inner enumerations are never empty. Both the outer and inner
        /// enumerations may be lazy and should not be repeatedly evaluated.
        /// </remarks>
        ///
        /// <returns>
        /// Actions grouped by those that are very similar. Groups never
        /// repeat actions and actions are never repeated across groups
        /// </returns>
        ///
        IEnumerable<IEnumerable<IAction>> GenerateValidActionVariations
            (GameState gameState);
    }
}