using Microsoft.Xna.Framework;
using System;
using System.Linq;
using static MacGame.Game1;

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

            bool showedAHintThatYouDidNotGetYet = false;
            int index = 0;

            // Create a menu option for each coin hint.
            foreach (var hint in nextLevelInfo.CoinHints)
            {
                var alreadyGotCoin = (Game1.Player.StorageState.LevelsToCoins.ContainsKey(nextLevelInfo.LevelNumber) && Game1.Player.StorageState.LevelsToCoins[nextLevelInfo.LevelNumber].Contains(hint.Key));
               
                if (alreadyGotCoin || !showedAHintThatYouDidNotGetYet)
                {
                    var option = AddOption(hint.Value, (a, b) =>
                    {
                        var door = (OpenCloseDoor)Game1.CurrentLevel.Doors.Single(d => d.Name == doorNameEntered);
                        door.OpenThenCloseThenTransition(hint.Key);
                        Cancel(this, EventArgs.Empty);
                    });

                    if (alreadyGotCoin)
                    {
                        option.Color = Color.LightGray;
                    }
                    else
                    {
                        showedAHintThatYouDidNotGetYet = true;
                        // Select the first entry that you haven't gotten yet.
                        defaultSelectedEntryIndex = index;
                    }
                }
                index++;
            }
            
            AddOption("Back", (a, b) => {
                game.TransitionToState(Game1.GameState.Playing, TransitionType.Instant);
                Cancel(this, EventArgs.Empty);
            });

        }
    }
}
