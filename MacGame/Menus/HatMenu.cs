using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class HatMenu : Menu
    {
        private MenuOption _pilgrimOption;
        private MenuOption _ninjaOption;

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

            _pilgrimOption = AddOption("Pilgrim Hat", (a, b) =>
            {
                PlayOptionSelectedSound();
                Game1.Player.CurrentHat = Game1.Player.PilgrimHat;
            });

            _ninjaOption = AddOption("Ninja Hat", (a, b) =>
            {
                PlayOptionSelectedSound();
                Game1.Player.CurrentHat = Game1.Player.NinjaHat;
            });
        }

        public override void AddedToMenuManager()
        {
            _pilgrimOption.Hidden = !Game1.StorageState.HasPilgrimHat;
            _ninjaOption.Hidden = !Game1.StorageState.HasNinjaHat;
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
