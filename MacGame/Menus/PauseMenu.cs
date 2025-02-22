﻿using Microsoft.Xna.Framework;
using System;

namespace MacGame
{
    public class PauseMenu : Menu
    {

        MenuOption backToHub;
        MenuOption toggleFullScreen;

        public PauseMenu(Game1 game)
            : base(game)
        {

            IsDismissable = true;
            this.IsOverlay = false;

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (Game1.GAME_Y_RESOLUTION * 0.25f).ToInt());

            var confirmExitGame = new YesNoMenu(Game, "Exit to title screen.\n   Are you sure?", (a, b) =>
            {
                this.Game.GoToTitleScreen();
            });
            confirmExitGame.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, 120);
            confirmExitGame.Scale = this.Scale;

            AddOption("Back", (a, b) => {
                PlayOptionSelectedSound();
                game.Unpause();
            });

            if (Game1.IS_DEBUG)
            {
                var restart = AddOption("Restart", (a, b) => {
                    PlayOptionSelectedSound();
                    Game.RestartLevel();
                });
            }

            backToHub = AddOption("Back to Hub", (a, b) => {
                PlayOptionSelectedSound();
                Game.GoToHub(true);
            });

            Func<string> GetFullScreenText = () => Game.IsFullScreen() ? "Windowed" : "Full Screen";

            toggleFullScreen = AddOption(GetFullScreenText(), (a, b) => {
                PlayOptionSelectedSound();
                Game.ToggleFullScreen();
                toggleFullScreen.Text = GetFullScreenText();
            });

            AddOption("Quit", (a, b) =>
            {
                MenuManager.AddMenu(confirmExitGame);
            });
           
        }

        public override void AddedToMenuManager()
        {
            backToHub.Hidden = Game1.CurrentLevel.LevelNumber <= 0;
            CenterMenuAndChoices();
            base.AddedToMenuManager();
        }

        public void SetupTitle(string title)
        {
            this.menuTitle = title;
        }

        protected override void OnCancel()
        {
            PlayOptionSelectedSound();
            Game.Unpause();
        }
    }
}
