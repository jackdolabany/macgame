using Microsoft.Xna.Framework;

namespace MacGame
{
    public class PauseMenu : Menu
    {
        public PauseMenu(Game1 game)
            : base(game)
        {

            IsDismissable = true;
            this.IsOverlay = false;

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION / 2 - 10);
            this.Scale = 1f;

            var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to exit to\nthe title screen?", (a,b) =>
            {
                this.Game.GoToTitleScreen();
            });
            confirmExitGame.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, 30);
            confirmExitGame.Scale = this.Scale;

            var saveMenu = new SaveMenu(game);
            var loadMenu = new LoadMenu(game);

            AddOption("Save", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                MenuManager.AddMenu(saveMenu);
            });

            AddOption("Load", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                MenuManager.AddMenu(loadMenu);
            });

            AddOption("Back", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                game.Unpause();
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
