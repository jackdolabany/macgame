using Microsoft.Xna.Framework;

namespace MacGame
{
    public class DeadMenu : Menu
    {
        public DeadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Game Over";
            
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 3f)));
            
            AddOption("Back to Hub", (a, b) => Game.GoToHub());
            AddOption("Title Screen", (a, b) => Game.GoToTitleScreen());

           
        }
    }
}
