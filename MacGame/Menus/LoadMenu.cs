using Microsoft.Xna.Framework;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace MacGame
{
    public class LoadMenu : Menu
    {

        DeleteMenu deleteMenu;

        public LoadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Load Game";
            
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 4f)));

            deleteMenu = new DeleteMenu(Game, this);

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
    }
}
