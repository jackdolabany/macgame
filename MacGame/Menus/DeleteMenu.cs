using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class DeleteMenu : Menu
    {

        private AlertBoxMenu alertDeletedMenu;

        private LoadMenu loadMenu;

        public DeleteMenu(Game1 game, LoadMenu loadMenu) : base(game)
        {
            this.menuTitle = "Delete";
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 4f)));

            alertDeletedMenu = new AlertBoxMenu(this.Game, "The game has\nbeen deleted", (a, b) => AfterDelete());
            alertDeletedMenu.IsOverlay = true;
            this.loadMenu = loadMenu;
        }

        public void Initialize(StorageState? file1, StorageState? file2, StorageState? file3)
        {
            loadMenu.File1 = file1;
            loadMenu.File2 = file2;
            loadMenu.File3 = file3;

            loadMenu.SlotToState[1] = file1;
            loadMenu.SlotToState[2] = file2;
            loadMenu.SlotToState[3] = file3;

            this.menuOptions.Clear();

            SetupMenuForStorageState(loadMenu.File1, 1);
            SetupMenuForStorageState(loadMenu.File2, 2);
            SetupMenuForStorageState(loadMenu.File3, 3);

            AddOption("Back", (a, b) => BackToLoadMenu());

            isPositioned = false;

            base.AddedToMenuManager();

        }

        private MenuOption SetupMenuForStorageState(StorageState? state, int slotNumber)
        {
            if (state == null)
            {
                var alertBox = new AlertBoxMenu(this.Game, "    File is\nalready empty", Cancel);
                alertBox.IsOverlay = true;
                return AddOption($"Empty {slotNumber}", (a, b) => MenuManager.AddMenu(alertBox));
            }
            else
            {
                var ConfirmDelete = new YesNoMenu(this.Game, $"   Are you sure\n    you want to\ndelete this game?", (a, b) => DeleteGame(slotNumber));
                ConfirmDelete.IsOverlay = true;
                return AddOption($"Delete {slotNumber}", (a, b) => MenuManager.AddMenu(ConfirmDelete));
            }
        }

        public void DeleteGame(int slot)
        {
            StorageManager.TryDeleteGame(slot);
            loadMenu.SlotToState[slot] = null;

            switch (slot)
            {
                case 1:
                    loadMenu.File1 = null;
                    break;
                case 2:
                    loadMenu.File2 = null;
                    break;
                case 3:
                    loadMenu.File3 = null;
                    break;
            }

            MenuManager.AddMenu(alertDeletedMenu);

        }

        public void AfterDelete()
        {
            // Re-initialize with the now deleted file.
            Initialize(loadMenu.File1, loadMenu.File2, loadMenu.File3);
            
            // Remove this alert saying the file was deleted.
            MenuManager.RemoveTopMenu();

            // Remove the confirmation choice menu.
            MenuManager.RemoveTopMenu();

            BackToLoadMenu();
        }

        public void BackToLoadMenu()
        {
            loadMenu.Initialize(loadMenu.File1, loadMenu.File2, loadMenu.File3);
            MenuManager.RemoveTopMenu();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            // Draw over the background.
            var screenRect = new Rectangle(0, 0, Game1.GAME_X_RESOLUTION, Game1.GAME_Y_RESOLUTION);
            spriteBatch.Draw(Game1.TileTextures, screenRect, Game1.WhiteSourceRect, Color.DarkRed, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            StorageState? selectedState = null;
            if (loadMenu.SlotToState.ContainsKey(this.selectedEntryIndex + 1))
            {
                selectedState = loadMenu.SlotToState[this.selectedEntryIndex + 1];
            }

            // Draw a black dialog box to the right for stats
            LoadMenu.DrawLoadMenuDialogBox(spriteBatch, this.DrawDepth, selectedState);
            
            base.Draw(spriteBatch);
        }
    }
}
