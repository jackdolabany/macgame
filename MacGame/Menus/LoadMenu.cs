using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class LoadMenu : Menu
    {

        DeleteMenu deleteMenu;

        public LoadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Load Game";

            // 7 tiles of border to the right
            var statsAreaWidth = Game1.TileSize * 7;
            var menuXPosition = (Game1.GAME_X_RESOLUTION - statsAreaWidth) / 2;

            this.Position = new Vector2(menuXPosition, 100);

            deleteMenu = new DeleteMenu(Game, this);
            deleteMenu.Position = this.Position;

        }

        private MenuOption SetupMenuForStorageState(StorageState? state, int slotNumber)
        {
            if (state == null)
            {
                return AddOption($"New Game {slotNumber}", (a, b) => LoadGame(slotNumber));
            }
            else
            {
                return AddOption($"Continue {slotNumber}", (a, b) => LoadGame(slotNumber));
            }
        }

        /// <summary>
        /// Normally we'd set up the menu options in the constructor but we need
        /// to reload the storage state each time since you have have saved since it
        /// was last constructed.
        /// </summary>
        public override void AddedToMenuManager()
        {
            var task1 = StorageManager.LoadStorageStateForSlot(1);
            var task2 = StorageManager.LoadStorageStateForSlot(2);
            var task3 = StorageManager.LoadStorageStateForSlot(3);

            task1.Wait();
            task2.Wait();
            task3.Wait();

            Initialize(task1.Result, task2.Result, task3.Result);

            base.AddedToMenuManager();
        }

        public void Initialize(StorageState? file1, StorageState? file2, StorageState? file3)
        {
            this.menuOptions.Clear();

            SetupMenuForStorageState(file1, 1);
            SetupMenuForStorageState(file2, 2);
            SetupMenuForStorageState(file3, 3);

            if (file1 != null || file2 != null || file3 != null)
            {
                AddOption("Delete", (a, b) => {
                    deleteMenu.Initialize(file1, file2, file3);
                    MenuManager.AddMenu(deleteMenu);
                });
            }
            AddOption("Back", Cancel);

            isPositioned = false;
        }

        public void LoadGame(int slot)
        {
            StorageManager.TryLoadGame(slot);
            Game.Unpause();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw over the background.
            var screenRect = new Rectangle(0, 0, Game1.GAME_X_RESOLUTION, Game1.GAME_Y_RESOLUTION);
            spriteBatch.Draw(Game1.TileTextures, screenRect, Game1.WhiteSourceRect, Color.Gray, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            // Draw a black dialog box to the right for stats
            DrawLoadMenuDialogBox(spriteBatch, this.DrawDepth);

            // Write some stats for the selected file.

            base.Draw(spriteBatch);
        }

        public static void DrawLoadMenuDialogBox(SpriteBatch spriteBatch, float menuDrawDepth)
        {
            // Draw a black dialog box to the right for stats
            var tileWidth = 7;
            var tileHeight = 10;

            int x = Game1.GAME_X_RESOLUTION - (tileWidth * Game1.TileSize) - 16;

            var height = (tileHeight * Game1.TileSize);
            int y = (Game1.GAME_Y_RESOLUTION - height) / 2;

            // Put it behind this menu a bit
            var drawDepth = menuDrawDepth + Game1.MIN_DRAW_INCREMENT * 100;

            ConversationManager.DrawDialogBox(spriteBatch, new Vector2(x, y), tileWidth, tileHeight, drawDepth);
        }
    }
}
