using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class HatMenu : Menu
    {
        public override SpriteFont MenuItemFont => Game1.FontSmall;

        public HatMenu(Game1 game) : base(game)
        {
            menuTitle = "Hats";
            IsDismissable = true;
            IsOverlay = false;

            AddOption("None", (a, b) =>
            {
                PlayOptionSelectedSound();
                Game1.Player.CurrentHat = null;
            });

            foreach (var hat in Game1.Player.Hats)
            {
                var h = hat;
                AddOption(h.HatName, (a, b) =>
                {
                    PlayOptionSelectedSound();
                    Game1.Player.CurrentHat = h;
                });
            }
        }

        public override void AddedToMenuManager()
        {
            // menuOptions[0] is "None" — always visible
            // menuOptions[i+1] corresponds to Hats[i]
            for (int i = 0; i < Game1.Player.Hats.Count; i++)
            {
                menuOptions[i + 1].Hidden = !Game1.StorageState.CollectedHats.Contains(Game1.Player.Hats[i].HatName);
            }
            CenterMenuAndChoices();
            base.AddedToMenuManager();
        }

        protected override void OnCancel()
        {
            Game1.StorageState.SelectedHat = Game1.Player.CurrentHat?.HatName ?? "None";
            StorageManager.TrySaveGame();
            base.OnCancel();
        }
    }
}
