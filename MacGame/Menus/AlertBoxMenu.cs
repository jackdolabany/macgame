﻿using System;
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

            this.Position = new Vector2(Game1.Camera.ViewPortWidth / 2, (Game1.Camera.ViewPortHeight * 0.333f).ToInt());

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
