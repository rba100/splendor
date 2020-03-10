using System.Collections.Generic;

namespace Splendor.Core
{
    public interface IGameInitialiser
    {
        GameState Create(IEnumerable<string> playerNames);
    }
}
