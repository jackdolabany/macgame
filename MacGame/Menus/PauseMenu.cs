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

            this.Position = new Vector2(20, 20);
            this.Scale = 0.8f;

            var confirmExitGame = new YesNoMenu(Game, "Are you sure you want to exit to the title screen?\n             Any unsaved progress will be lost.", (a,b) =>
            {
                this.Game.GoToTitleScreen();
                //SoundManager.PlaySound("rollover2");
            });

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
