using System;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class AlertBoxMenu : Menu
    {
        public AlertBoxMenu(Game1 game, string title, EventHandler<MenuEventArgs> acceptEvent)
            : base(game)
        {
            this.menuTitle = title;

            var ok = new MenuOption("OK", this);

            ok.Chosen += acceptEvent;

            this.Position = new Vector2(Game1.Camera.ViewPortWidth / 2, (int)(Game1.Camera.ViewPortHeight * (1f / 3f)));

            this.menuOptions.Add(ok);
            this.IsOverlay = true;
        }

        public override void AddedToMenuManager()
        {
            base.AddedToMenuManager();
            SoundManager.PlaySound("AlertBox");
        }
    }
}
