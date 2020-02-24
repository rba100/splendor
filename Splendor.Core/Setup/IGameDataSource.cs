using System.Collections.Generic;

namespace Splendor
{
    public interface IGameDataSource
    {
        IReadOnlyCollection<Card> AllCards();
        IReadOnlyCollection<Noble> AllNobles();
    }
}
