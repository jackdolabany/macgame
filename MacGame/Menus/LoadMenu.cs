using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MacGame
{
    public class LoadMenu : Menu
    {

        DeleteMenu deleteMenu;

        public StorageState? File1 { get; set; }
        public StorageState? File2 { get; set; }
        public StorageState? File3 { get; set; }

        public Dictionary<int, StorageState?> SlotToState = new Dictionary<int, StorageState?>();

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

            this.File1 = file1;
            this.File2 = file2;
            this.File3 = file3;

            SlotToState[1] = file1;
            SlotToState[2] = file2;
            SlotToState[3] = file3;

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
            var backgroundColor = new Color(0x1D, 0x2B, 0x53, 0xFF);
            spriteBatch.Draw(Game1.TileTextures, screenRect, Game1.WhiteSourceRect, backgroundColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            StorageState? selectedState = null;
            if (SlotToState.ContainsKey(this.selectedEntryIndex + 1))
            {
                selectedState = SlotToState[this.selectedEntryIndex + 1];
            }

            // Draw a black dialog box to the right for stats
            DrawLoadMenuDialogBox(spriteBatch, this.DrawDepth, selectedState);

            // Write some stats for the selected file.

            base.Draw(spriteBatch);
        }

        public static void DrawLoadMenuDialogBox(SpriteBatch spriteBatch, float menuDrawDepth, StorageState? state)
        {
            // Draw a black dialog box to the right for stats
            var tileWidth = 7;
            var tileHeight = 7;

            int x = Game1.GAME_X_RESOLUTION - (tileWidth * Game1.TileSize) - 16;

            var height = (tileHeight * Game1.TileSize);
            int y = (Game1.GAME_Y_RESOLUTION - height) / 2;

            // Put it behind this menu a bit
            var drawDepth = menuDrawDepth + Game1.MIN_DRAW_INCREMENT * 100;

            ConversationManager.DrawDialogBox(spriteBatch, new Vector2(x, y), tileWidth, tileHeight, drawDepth);

            drawDepth -= Game1.MIN_DRAW_INCREMENT;

            if (state == null)
            {
                var text = "Empty";
                var position = new Vector2(x + 52, y + 52);
                spriteBatch.DrawString(Game1.Font, text, position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, drawDepth);
            }
            else
            {
                var sockCount = state.Levels.Select(l => l.Value.CollectedSocks.Count).Sum();

                var percentageComplete = (int)System.Math.Round((float)sockCount / (float)Game1.TotalSocks * 100f);

                // offset for stats
                x += 16;
                y += 44;

                // Draw the sock
                var sockSourceRect = Helpers.GetTileRect(9, 2);
                spriteBatch.Draw(Game1.TileTextures, new Rectangle(8 + x, 8 + y, Game1.TileSize, Game1.TileSize), sockSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, drawDepth);

                // Draw sock count
                var sockText = $"{sockCount}\n";
                spriteBatch.DrawString(Game1.Font, sockText, new Vector2(x + 44, y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, drawDepth);

                // Draw a crown to the right of the socks if they've beaten the game
                if (state.HasBeatedGame)
                {
                    var crownSourceRect = Helpers.GetTileRect(12, 2);
                    spriteBatch.Draw(Game1.TileTextures, new Rectangle(x + 84, y - 36, Game1.TileSize, Game1.TileSize), crownSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, drawDepth);
                }

                // Other stats
                var totalPlayTime = TimeSpan.FromSeconds(state.TotalElapsedTime);
                string totalPlayTimeText;
                if (totalPlayTime > TimeSpan.FromDays(1))
                {
                    totalPlayTimeText = $"{totalPlayTime:dd\\:hh\\:mm}";
                }
                else
                {                  
                    totalPlayTimeText = $"{totalPlayTime:hh\\:mm}";
                }

                var statText = $"{percentageComplete.ToString("00")}%\n" +
                    totalPlayTimeText;

                spriteBatch.DrawString(Game1.Font, statText, new Vector2(x + 16, y + 44), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, drawDepth);

            }

        }
    }
}
