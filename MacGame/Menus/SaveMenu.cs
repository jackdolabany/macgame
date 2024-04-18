using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace MacGame
{
    public class SaveMenu : Menu
    {

        public Menu[] SaveSlotMenus = new Menu[3];

        public SaveMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Save Game";
            
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 3f)));

            AddOption("Save Slot 1", (a, b) => ConfirmSave(1));
            AddOption("Save Slot 2", (a, b) => ConfirmSave(2));
            AddOption("Save Slot 3", (a, b) => ConfirmSave(3));
            AddOption("Back", Cancel);

            for (int i = 0; i < 3; i++)
            {
                var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to overwrite the data?", (a, b) =>
                {
                    Save(i);
                });
                SaveSlotMenus[i] = confirmExitGame;
            }
        }

        public void ConfirmSave(int slot)
        {
            // TODO: Check if there's data there first.
            MenuManager.AddMenu(SaveSlotMenus[slot - 1]);
        }

        public void Save(int slot)
        {
            StorageManager.TrySaveGame(slot);
            MenuManager.AddMenu(new AlertBoxMenu(this.Game, "Game has been saved", SaveComplete));
        }

        public void SaveComplete(object sender, MenuEventArgs e)
        {
            Game.Unpause();
        }
    }
}
