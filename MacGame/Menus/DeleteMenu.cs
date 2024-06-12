using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class DeleteMenu : Menu
    {

        private StorageState? File1 { get; set; }
        private StorageState? File2 { get; set; }
        private StorageState? File3 { get; set; }

        private AlertBoxMenu alertDeletedMenu;

        private LoadMenu loadMenu;

        public DeleteMenu(Game1 game, LoadMenu loadMenu) : base(game)
        {
            this.menuTitle = "Delete Saved Game";
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 4f)));

            alertDeletedMenu = new AlertBoxMenu(this.Game, "The game has been deleted", (a, b) => AfterDelete());
            this.loadMenu = loadMenu;
        }

        public void Initialize(StorageState? file1, StorageState? file2, StorageState? file3)
        {
            this.File1 = file1;
            this.File2 = file2;
            this.File3 = file3;

            this.menuOptions.Clear();

            SetupMenuForStorageState(File1, 1);
            SetupMenuForStorageState(File2, 2);
            SetupMenuForStorageState(File3, 3);

            AddOption("Back", (a, b) => BackToLoadMenu());

            isPositioned = false;

            base.AddedToMenuManager();

        }

        private MenuOption SetupMenuForStorageState(StorageState? state, int slotNumber)
        {
            if (state == null)
            {
                var alertBox = new AlertBoxMenu(this.Game, "File is already empty", Cancel);

                return AddOption($"Empty Game {slotNumber}", (a, b) => MenuManager.AddMenu(alertBox));
            }
            else
            {
                var ConfirmDelete = new YesNoMenu(this.Game, $"Are you sure you want to delete this game?", (a, b) => DeleteGame(slotNumber));
                return AddOption($"Delete Game {slotNumber}", (a, b) => MenuManager.AddMenu(ConfirmDelete));
            }
        }

        public void DeleteGame(int slot)
        {
            StorageManager.TryDeleteGame(slot);
            switch (slot)
            {
                case 1:
                    File1 = null;
                    break;
                case 2:
                    File2 = null;
                    break;
                case 3:
                    File3 = null;
                    break;
            }

            MenuManager.AddMenu(alertDeletedMenu);

        }

        public void AfterDelete()
        {
            // Re-initialize with the now deleted file.
            Initialize(this.File1, this.File2, this.File3);
            
            // Remove this alert saying the file was deleted.
            MenuManager.RemoveTopMenu();

            // Remove the confirmation choice menu.
            MenuManager.RemoveTopMenu();
        }

        public void BackToLoadMenu()
        {
            loadMenu.Initialize(File1, File2, File3);
            MenuManager.RemoveTopMenu();
        }
    }
}
