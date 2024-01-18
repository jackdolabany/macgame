using Microsoft.Xna.Framework;

namespace MacGame
{
    public class MainMenu : Menu
    {
        Menu confirmMenu;

        public MainMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "";

            AddOption("New Game", (sender, args) => game.StartNewGame());

            confirmMenu = new YesNoMenu(Game, "Are you sure you want to \n        exit the game?", (sender, args) => this.Game.Exit());

            AddOption("Quit", (sender, args) => {
                confirmMenu.Scale = this.Scale;
                MenuManager.AddMenu(confirmMenu);
            });

            this.Scale = 0.6f;
            this.IsOverlay = false;
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION * 0.70f);
        }

    }
}
