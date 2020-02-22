# Splendor simulator

Write and battle Splender AI.

    public interface ISpendorAi
    {
        string Name { get; }
        IAction ChooseAction(GameState gameState);
    }

Implement one of those. Look at `StupidSplendorAi` to see some of the helper methods.

## Rules for implementation

 - When `ChooseAction` is called on your AI, it's guarenteed to be the turn of that AI.
 - You may choose to return a new instance of `GameState` or mutate and return the one passed in.

