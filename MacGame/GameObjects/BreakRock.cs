﻿using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A rock that you stand on and it breaks. Mac can't help but fall and this can be used
    /// as a one way door kinda thing. It'll reform and block again if nothing is colliding with it.
    /// </summary>
    public class BreakRock : GameObject
    {
        int _cellX;
        int _cellY;

        public bool IsBroken { get; set; } = true;

        private bool _isInitialized = false;

        NoDisplay noDisplay;

        public string GroupName { get; set; }

        /// <summary>
        /// Normal behavior is to save the state of these when they break. For bosses or other things you
        /// might want it to reset every time. Set the OverrideSave property to "1" in the map to set this.
        /// </summary>
        public bool OverrideSave { get; set; } = false;

        public BreakRock(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetCenteredCollisionRectangle(8, 8);

            var textures = content.Load<Texture2D>(@"Textures\Textures");

            noDisplay = new NoDisplay();

            if (IsBroken)
            {
                DisplayComponent = noDisplay;
            }
            else
            {
                DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(9, 0));
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = IsBroken;
            }
        }

        public void Break()
        {
            if (IsBroken) return;

            EffectsManager.AddExplosion(this.CollisionCenter);

            IsBroken = true;
            this.DisplayComponent = noDisplay;
            Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = true;

            SoundManager.PlaySound("Break");

            if (!OverrideSave)
            {
                Game1.StorageState.AddUnblockedMapSquare(Game1.CurrentLevel.LevelNumber, Game1.CurrentLevel.Name, _cellX, _cellY);
            }
        }

    }
}
