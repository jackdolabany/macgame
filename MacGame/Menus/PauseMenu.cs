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

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION * (1f / 2f));
            this.Scale = 0.5f;

            var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to exit to\nthe title screen?", (a,b) =>
            {
                this.Game.GoToTitleScreen();
            });
            confirmExitGame.Position = this.Position;

            confirmExitGame.Scale = this.Scale;

            AddOption("Back", (a, b) => {
                //SoundManager.PlaySound("rollover2");
                game.Unpause();
            });

            AddOption("Quit", (a, b) =>
            {
                MenuManager.AddMenu(confirmExitGame);
            });
        }

        protected override void OnCancel()
        {
            //SoundManager.PlaySound("rollover2");
            Game.Unpause();
        }
    }
}
