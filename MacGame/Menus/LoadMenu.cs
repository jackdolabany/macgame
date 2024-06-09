using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace MacGame
{
    public class LoadMenu : Menu
    {

        public LoadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Load Game";
            
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 4f)));

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
            this.menuOptions.Clear();

            var task1 = StorageManager.LoadStorageStateForSlot(1);
            var task2 = StorageManager.LoadStorageStateForSlot(2);
            var task3 = StorageManager.LoadStorageStateForSlot(3);

            task1.Wait();
            task2.Wait();
            task3.Wait();

            SetupMenuForStorageState(task1.Result, 1);
            SetupMenuForStorageState(task2.Result, 2);
            SetupMenuForStorageState(task3.Result, 3);

            AddOption("Back", Cancel);

            isPositioned = false;

            base.AddedToMenuManager();
        }

        public void LoadGame(int slot)
        {
            StorageManager.TryLoadGame(slot);
            Game.Unpause();
        }
    }
}
