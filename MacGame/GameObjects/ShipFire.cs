using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace MacGame
{

    
    /// <summary>
    /// The fire from the rocket in the back of your spaceship.
    /// </summary>
    public class ShipFire : GameObject
    {

        private AnimationDisplay AnimationDisplay => (AnimationDisplay)DisplayComponent;

        public ShipFire(Texture2D textures)
        {
            // We'll draw the wings all custom like.
            var ad = new AnimationDisplay();
            var fire = new AnimationStrip(textures, Helpers.GetTileRect(5, 1), 3, "fire");
            fire.LoopAnimation = false;
            fire.FrameLength = 0.1f;

            ad.Add(fire);

            this.DisplayComponent = ad;

            Enabled = false;

            SetWorldLocationCollisionRectangle(4, 4);
            this.collisionRectangle.Y -= 8;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Enabled)
            {
                if (AnimationDisplay.CurrentAnimation.FinishedPlaying)
                {
                    Enabled = false;
                }

                if (AnimationDisplay.CurrentAnimation.currentFrameIndex == 0)
                {
                    DisplayComponent.TintColor = Color.White * 0.9f;
                }
                else if (AnimationDisplay.CurrentAnimation.currentFrameIndex == 1)
                {
                    DisplayComponent.TintColor = Color.White * 0.6f;
                }
                else if (AnimationDisplay.CurrentAnimation.currentFrameIndex == 2)
                {
                    DisplayComponent.TintColor = Color.White * 0.3f;
                }
            }
            base.Update(gameTime, elapsed);
        }

        public void Reset()
        {
            AnimationDisplay.Play("fire");
            Enabled = true;
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
