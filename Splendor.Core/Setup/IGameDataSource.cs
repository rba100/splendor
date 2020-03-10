using System.Collections.Generic;

namespace Splendor.Core
{
    public interface IGameDataSource
    {
        IReadOnlyCollection<Card> AllCards();
        IReadOnlyCollection<Noble> AllNobles();
    }
}
