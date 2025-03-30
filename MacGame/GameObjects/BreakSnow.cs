using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A passable snow brick that breaks when you stand on it. It won't reform.
    /// </summary>
    public class BreakSnow : GameObject
    {
        int _cellX;
        int _cellY;

        private bool _isBroken = false;
        private bool _isBreaking = false;

        AnimationDisplay AnimationDisplay => DisplayComponent as AnimationDisplay;

        public BreakSnow(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 8);

            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(10, 18), 1, "idle");
            idle.LoopAnimation = false;
            ad.Add(idle);

            var steppedOn = new AnimationStrip(textures, Helpers.GetTileRect(11, 18), 1, "steppedOn");
            steppedOn.LoopAnimation = false;
            ad.Add(steppedOn);

            var breakUp = new AnimationStrip(textures, Helpers.GetTileRect(11, 18), 4, "breakUp");
            breakUp.LoopAnimation = false;
            breakUp.FrameLength = 0.1f;
            ad.Add(breakUp);

            ad.Play("idle");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (!_isBroken && !_isBreaking)
            {
                // Check if player is above.
                var aboveCollisionRectangle = new Rectangle(CollisionRectangle.X, CollisionRectangle.Y - 1, CollisionRectangle.Width, 1);
                if (Game1.Player.CollisionRectangle.Intersects(aboveCollisionRectangle))
                {
                    Break();
                }
            }

            if (AnimationDisplay.CurrentAnimationName == "breakUp" && AnimationDisplay.CurrentAnimation.FinishedPlaying)
            {
                this.Enabled = false;
            }
        }

        public void Break()
        {
            if (_isBroken || _isBreaking) return;

            _isBreaking = true;

            AnimationDisplay.Play("steppedOn");
            SoundManager.PlaySound("Break", 1f, 0.2f);

            TimerManager.AddNewTimer(1f, () =>
            {
                AnimationDisplay.Play("breakUp");
                Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = true;
                SoundManager.PlaySound("Break");
                _isBroken = true;
            });
           
        }

    }
}
