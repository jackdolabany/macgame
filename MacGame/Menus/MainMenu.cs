using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class MainMenu : Menu
    {
        Menu confirmMenu;
        Menu loadMenu;

        public MainMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "";

            loadMenu = new LoadMenu(game);

            AddOption("Play", (sender, args) => {
                MenuManager.AddMenu(loadMenu);
                PlayOptionSelectedSound();
            });

            confirmMenu = new YesNoMenu(Game, "  Exit Game. \nAre you sure?", (sender, args) => this.Game.Exit());
            confirmMenu.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, 120);

            AddOption("Quit", (sender, args) => {
                confirmMenu.Scale = this.Scale;
                MenuManager.AddMenu(confirmMenu);
            });

            this.IsOverlay = false;
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * 0.66f));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw a black dialog box to the right for stats
            var tileWidth = 6;
            var tileHeight = 4;

            int x = (Game1.GAME_X_RESOLUTION - (tileWidth * Game1.TileSize)) / 2;

            int y = (int)this.Position.Y - 46;

            // Put it behind this menu a bit
            var drawDepth = this.DrawDepth + Game1.MIN_DRAW_INCREMENT * 100;

            ConversationManager.DrawDialogBox(spriteBatch, new Vector2(x, y), tileWidth, tileHeight, drawDepth);

            base.Draw(spriteBatch);
        }

    }
}
