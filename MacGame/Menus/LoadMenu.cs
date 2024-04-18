using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace MacGame
{
    public class LoadMenu : Menu
    {

        public Menu[] LoadSlotMenus = new Menu[3];

        public LoadMenu(Game1 game)
            : base(game)
        {
            this.menuTitle = "Load Game";
            
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * (1f / 3f)));

            AddOption("Load Slot 1", (a, b) => ConfirmLoad(1));
            AddOption("Load Slot 2", (a, b) => ConfirmLoad(2));
            AddOption("Load Slot 3", (a, b) => ConfirmLoad(3));
            AddOption("Back", Cancel);

            for (int i = 0; i < 3; i++)
            {
                var confirmExitGame = new YesNoMenu(Game, "Are you sure you \n want to load this game?", (a, b) =>
                {
                    Load(i);
                });
                LoadSlotMenus[i] = confirmExitGame;
            }
        }

        public void ConfirmLoad(int slot)
        {
            // TODO: Check if there's data there first.
            MenuManager.AddMenu(LoadSlotMenus[slot - 1]);
        }

        public void Load(int slot)
        {
            StorageManager.TryLoadGame(slot);
            Game.Unpause();
        }
    }
}
