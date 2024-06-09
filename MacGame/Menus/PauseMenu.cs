﻿using Microsoft.Xna.Framework;

namespace MacGame
{
    public class PauseMenu : Menu
    {
        public PauseMenu(Game1 game)
            : base(game)
        {

            IsDismissable = true;
            this.IsOverlay = false;

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION * 0.333f);
            this.Scale = 1f;

            var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to exit to\nthe title screen?", (a,b) =>
            {
                this.Game.GoToTitleScreen();
            });
            confirmExitGame.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, 120);
            confirmExitGame.Scale = this.Scale;

            AddOption("Back", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                game.Unpause();
            });

            // TODO: Dont' show this if you're in the hub
            var option = AddOption("Back to Hub", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                Game.GoToHub(true);
            });

            AddOption("Quit", (a, b) =>
            {
                MenuManager.AddMenu(confirmExitGame);
            });
           
        }

        public void SetupTitle(string title)
        {
            this.menuTitle = title;
        }

        protected override void OnCancel()
        {
            //SoundManager.PlaySound("rollover2");
            Game.Unpause();
        }
    }
}
