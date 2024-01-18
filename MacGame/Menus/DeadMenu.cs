using Microsoft.Xna.Framework;

namespace MacGame
{
    public class DeadMenu : Menu
    {
        public DeadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "You died.";
            this.Scale = 0.8f;

            AddOption("Restart", (a, b) => Game.StartNewGame());
            AddOption("Title Screen", (a, b) => Game.GoToTitleScreen());

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION * (1f / 3f));
        }
    }
}
