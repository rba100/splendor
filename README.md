# Splendor simulator

Write and battle Splender AI.

    public interface ISpendorAi
    {
        string Name { get; }
        IAction ChooseAction(GameState gameState);
    }

Implement one of those. Look at `StupidSplendorAi` to see some of the helper methods.

## General rules for ChooseAction implementation

 - When `ChooseAction` is called on your AI, it's guarenteed to be the turn of that AI.
 - `ChooseAction` must only interact with the game by returning an action from the selection below. You must not modify any other state.

## Possible actions

 - `BuyCard` causes the purchase of either a reserved card or a card face-up on the board. It has some static helper methods you can use: `bool CanAffordCard(Player, Card)` to check whether that player has bonuses and coins sufficient to get the card at all, and `.CreateDefaultPaymentOrNull(Player, Card)` to let the game choose which coins to pay with, using gold from that player if necesary.
 - `TakeTokens` causes a transfer of tokens to / from the bank and the player. You can specify the tokens to return if you'd end up with more than ten.
 - `ReserveCard` causes the reservation of a face-up card. Gold is handled automatically.
 - `ReserveFaceDownCard` causes the reservation of the top face-down card of a specific tier. Gold is handled automatically.
 - `NoAction` causes the turn to be passed with no action. Not technically in the rules but here if your AI is stuck. If all players pass the turn in sequence, the game ends immediately.
