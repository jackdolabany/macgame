using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace MacGame.DisplayComponents
{
    public class AnimationDisplay : DisplayComponent
    {

        protected DrawObject drawObject;
        public Dictionary<string, AnimationStrip> animations = new Dictionary<string, AnimationStrip>();

        public AnimationDisplay()
            : base()
        {
            drawObject = new DrawObject();
            CurrentAnimationName = "";
        }

        public string CurrentAnimationName;

        public AnimationStrip? CurrentAnimation
        {
            get
            {
                if (animations.ContainsKey(CurrentAnimationName))
                {
                    return animations[CurrentAnimationName];
                }
                return null;
            }
        }

        public Vector2 WorldLocation
        {
            get { return drawObject.Position; }
            set { drawObject.Position = value; }
        }

        public void Add(string key, AnimationStrip animation)
        {
            animations.Add(key, animation);
        }

        public void Add(AnimationStrip animation)
        {
            animations.Add(animation.Name, animation);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (drawObject.Texture != null)
            {
                spriteBatch.Draw(
                    drawObject.Texture,
                    drawObject.Position.ToIntegerVector(),
                    drawObject.SourceRectangle,
                    TintColor,
                    Rotation,
                    RotationAndDrawOrigin,
                    Scale,
                    drawObject.Effect,
                    DrawDepth);
            }
        }

        public override void Update(GameTime gameTime, float elapsed, Vector2 position, bool flipped)
        {
            base.Update(gameTime, elapsed, position, flipped);

            // Update the animation
            if (!animations.ContainsKey(CurrentAnimationName)) return;

            var anim = animations[CurrentAnimationName];
            if (anim.FinishedPlaying)
            {
                if (!string.IsNullOrEmpty(anim.NextAnimation))
                {
                    Play(anim.NextAnimation);
                }
            }
            else
            {
                anim.Update(elapsed);
            }

            SpriteEffects effect = SpriteEffects.None;
            if (flipped)
            {
                effect = SpriteEffects.FlipHorizontally;
            }
            drawObject.Effect = effect;

            if (!animations.ContainsKey(CurrentAnimationName)) return;

            drawObject.Texture = anim.Texture;
            drawObject.SourceRectangle = anim.FrameRectangle;

            var center = GetWorldCenter(ref position);
            var drawPosition = center - new Vector2(drawObject.SourceRectangle.Width / 2, drawObject.SourceRectangle.Height / 2) * Scale;
            drawObject.Position = RotateAroundOrigin(drawPosition, GetWorldCenter(ref position), Rotation);

            // Lock them into an integer position.
            drawObject.Position = new Vector2(drawObject.Position.X, drawObject.Position.Y);

        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            AnimationStrip animationToCheck;
            if (CurrentAnimationName != "")
            {
                animationToCheck = animations[CurrentAnimationName];
            }
            else
            {
                animationToCheck = animations.Values.First();
            }

            return new Vector2(
              worldLocation.X,
              worldLocation.Y - animationToCheck.FrameHeight / 2f);
        }

        public AnimationStrip? Play(string name, int startFrame)
        {
            if (name != null && animations.ContainsKey(name))
            {
                CurrentAnimationName = name;
                animations[name].Play(startFrame);
                return animations[name];
            }

            if (Game1.IS_DEBUG)
            {
                throw new Exception("Animation not found: " + name);
            }

            return null;

        }

        public AnimationStrip? Play(string name)
        {
            return Play(name, 0);
        }

        public AnimationStrip? PlayIfNotAlreadyPlaying(string name)
        {
            if (CurrentAnimationName != name)
            {
                return Play(name, 0);
            }
            return null;
        }

        internal void StopPlaying()
        {
            CurrentAnimationName = "";
            this.drawObject.Texture = null;
        }
    }
}