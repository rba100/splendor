using System.Collections.Generic;

namespace Splendor
{
    public interface IGameInitialiser
    {
        GameState Create(IEnumerable<string> playerNames);
    }
}
