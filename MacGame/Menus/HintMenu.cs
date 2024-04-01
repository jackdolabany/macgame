using Microsoft.Xna.Framework;
using System;

namespace MacGame
{

    /// <summary>
    /// Represents the hints you see when you enter a sub world. You choose one hint and that's the star you're going after. 
    /// Doesn't need to be the one you get.
    /// </summary>
    public class HintMenu : Menu
    {
        public HintMenu(Game1 game, NextLevelInfo nextLevelInfo, string doorNameEntered) : base(game)
        {
            this.menuTitle = $"World {nextLevelInfo.LevelNumber} - {nextLevelInfo.Description}";

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 3f)));

            // Create a menu option for each coin hint.
            foreach (var hint in nextLevelInfo.CoinHints)
            {
                AddOption(hint.Value, (a, b) => {
                    GlobalEvents.FireDoorEntered(this, nextLevelInfo.MapName, "", doorNameEntered, hint.Key);
                    game.TransitionToState(Game1.GameState.Playing);
                    Cancel(this, EventArgs.Empty);
                });
            }
            
            AddOption("Back", (a, b) => {
                game.TransitionToState(Game1.GameState.Playing, false);
                Cancel(this, EventArgs.Empty);
            });

        }
    }
}
