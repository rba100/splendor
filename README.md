# Splendor simulator

Write and battle Splender AI.

    public interface ISpendorAi
    {
        string Name { get; }
        IAction ChooseAction(GameState gameState);
    }

Implement one of those. Look at `StupidSplendorAi` to see some of the helper methods.
