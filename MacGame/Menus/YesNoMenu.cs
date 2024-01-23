﻿using System;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class YesNoMenu : Menu
    {
        public YesNoMenu(Game1 game, string title, EventHandler<MenuEventArgs> acceptEvent)
            : base(game)
        {
            this.menuTitle = title;

            var yes = new MenuOption("Yes", this);
            var no = new MenuOption("No", this);
            
            yes.Chosen += acceptEvent;
            no.Chosen += Cancel;

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (int)(Game1.GAME_Y_RESOLUTION * 0.33f));

            this.menuOptions.Add(no);
            this.menuOptions.Add(yes);
            this.IsOverlay = true;
        }

        public override void AddedToMenuManager()
        {
            base.AddedToMenuManager();
            //SoundManager.PlaySound("chop");
        }
    }
}
