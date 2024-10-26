using Microsoft.Xna.Framework;

namespace MacGame
{
    public class PauseMenu : Menu
    {

        MenuOption backToHub;

        public PauseMenu(Game1 game)
            : base(game)
        {

            IsDismissable = true;
            this.IsOverlay = false;

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (Game1.GAME_Y_RESOLUTION * 0.333f).ToInt());
            this.Scale = 1f;

            var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to exit to\nthe title screen?", (a, b) =>
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

            AddOption("Quit", (a, b) =>
            {
                MenuManager.AddMenu(confirmExitGame);
            });
           
        }

        public override void AddedToMenuManager()
        {
            backToHub.Hidden = Game1.CurrentLevel.LevelNumber <= 0;
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
