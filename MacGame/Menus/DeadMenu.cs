﻿using Microsoft.Xna.Framework;

namespace MacGame
{
    public class DeadMenu : Menu
    {

        MenuOption backToHub;
        public DeadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Game Over";
            
            this.Position = new Vector2((Game1.GAME_X_RESOLUTION / 2f).ToInt(), (Game1.GAME_Y_RESOLUTION * (1f / 3f)).ToInt());

            if (Game1.IS_DEBUG)
            {
                var restart = AddOption("Restart", (a, b) => {
                    PlayOptionSelectedSound();
                    Game.RestartLevel();
                });
            }

            backToHub = AddOption("Back to Hub", (a, b) => Game.GoToHub(true));
            AddOption("Title Screen", (a, b) => Game.GoToTitleScreen());

        }

        public override void AddedToMenuManager()
        {
            backToHub.Hidden = Game1.CurrentLevel.LevelNumber <= 0;
            base.AddedToMenuManager();
        }
    }
}
