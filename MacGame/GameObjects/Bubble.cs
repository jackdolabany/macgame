using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace MacGame
{

    
    /// <summary>
    /// Just a bubble that floats up in water.
    /// </summary>
    public class Bubble : GameObject
    {

        public float LifeTimer = 0f;

        private AnimationDisplay AnimationDisplay => (AnimationDisplay)DisplayComponent;

        public Bubble(Texture2D textures)
        {
            // We'll draw the wings all custom like.
            var ad = new AnimationDisplay();
            var bubble = new AnimationStrip(textures, Helpers.GetTileRect(9, 29), 2, "bubble");
            bubble.LoopAnimation = true;
            bubble.FrameLength = 0.4f;

            ad.Add(bubble);

            var pop = new AnimationStrip(textures, Helpers.GetTileRect(11, 29), 1, "pop");
            pop.LoopAnimation = false;
            pop.FrameLength = 0.1f;
            ad.Add(pop);

            this.DisplayComponent = ad;
            ad.Play("bubble");

            Enabled = false;

            SetCenteredCollisionRectangle(4, 4);
            this.collisionRectangle.Y -= 8;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Enabled)
            {
                if (LifeTimer > 0f)
                {
                    LifeTimer -= elapsed;
                }
                else
                {
                    Pop();
                }

                // Pop if they hit something.
                if (OnLeftWall || OnRightWall || OnCeiling)
                {
                    Pop();
                }

                // Pop if they leave the water.
                var topLeft = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Left, this.CollisionRectangle.Top));
                var topRight = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Top));

                if (topLeft != null && !topLeft.IsWater || topRight != null && !topRight.IsWater)
                {
                    Pop();
                }

                if (AnimationDisplay.CurrentAnimationName == "pop" && AnimationDisplay.CurrentAnimation.FinishedPlaying)
                {
                    Enabled = false;
                }
            }
            base.Update(gameTime, elapsed);
        }

        public void Reset()
        {
            AnimationDisplay.Play("bubble");
            Enabled = true;
            LifeTimer = 5f;
            onLeftWall = false;
            onRightWall = false;
            onCeiling = false;
            onGround = false;
        }

        public void Pop()
        {
            AnimationDisplay.PlayIfNotAlreadyPlaying("pop");
            this.Velocity = Vector2.Zero;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                base.Draw(spriteBatch);
            }
        }

    }

}
