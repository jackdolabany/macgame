using Microsoft.Xna.Framework;
using System;

namespace MacGame
{
    public class GraphicsMenu : Menu
    {
        MenuOption toggleFullScreen;
        MenuOption shaderOption;

        public GraphicsMenu(Game1 game)
            : base(game)
        {
            IsDismissable = true;
            this.IsOverlay = false;

            this.menuTitle = "Graphics";
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (Game1.GAME_Y_RESOLUTION * 0.25f).ToInt());

            Func<string> GetFullScreenText = () => Game.IsFullScreen() ? "Windowed" : "Full Screen";

            toggleFullScreen = AddOption(GetFullScreenText(), (a, b) => {
                PlayOptionSelectedSound();
                Game.ToggleFullScreen();
                toggleFullScreen.Text = GetFullScreenText();
            });

            Func<string> GetShaderText = () => "CRT Filter: " + Game1.GetCRTModeName();

            shaderOption = AddOption(GetShaderText(), (a, b) => {
                PlayOptionSelectedSound();
                Game1.CycleCRTMode();
                shaderOption.Text = GetShaderText();
            });

            AddOption("Back", (a, b) => {
                PlayOptionSelectedSound();
                MenuManager.RemoveTopMenu();
            });
        }

        public override void AddedToMenuManager()
        {
            CenterMenuAndChoices();
            base.AddedToMenuManager();
        }

        protected override void OnCancel()
        {
            PlayOptionSelectedSound();
            MenuManager.RemoveTopMenu();
        }
    }
}
