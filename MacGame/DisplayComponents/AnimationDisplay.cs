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
            NextAnimationQueue = new Queue<string>();
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

        public void Add(string key, AnimationStrip animation)
        {
            animations.Add(key, animation);
        }

        public void Add(AnimationStrip animation)
        {
            animations.Add(animation.Name, animation);
        }

        /// <summary>
        /// Add another animation if you want it to play after this one immediately.
        /// </summary>
        public Queue<string> NextAnimationQueue { get; private set; }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {
            SpriteEffects effect = SpriteEffects.None;
            if (flipped)
            {
                effect = SpriteEffects.FlipHorizontally;
            }
            drawObject.Effect = effect;

            if (!animations.ContainsKey(CurrentAnimationName)) return;

            var anim = animations[CurrentAnimationName];
            drawObject.Texture = anim.Texture;
            drawObject.SourceRectangle = anim.FrameRectangle;

            var center = GetWorldCenter(ref position);
            
            var drawPosition = center - new Vector2(drawObject.SourceRectangle.Width / 2, drawObject.SourceRectangle.Height / 2) * Scale;

            drawObject.Position = RotateAroundOrigin(drawPosition, GetWorldCenter(ref position), Rotation);

            drawObject.Position += Offset;

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

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            // Update the animation
            if (!animations.ContainsKey(CurrentAnimationName)) return;

            var anim = animations[CurrentAnimationName];
            if (anim.FinishedPlaying)
            {
                if (NextAnimationQueue.Count > 0)
                {
                    Play(NextAnimationQueue.Dequeue());
                }
            }
            else
            {
                anim.Update(elapsed);
            }
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

        public AnimationDisplay Play(string name, int startFrame)
        {

            if (name != null && animations.ContainsKey(name))
            {
                CurrentAnimationName = name;
                animations[name].Play(startFrame);
                return this;
            }

            if (Game1.IS_DEBUG)
            {
                throw new Exception("Animation not found: " + name);
            }

            return this;

        }

        public AnimationDisplay Play(string name)
        {
            return Play(name, 0);
        }

        public AnimationDisplay PlayIfNotAlreadyPlaying(string name)
        {
            if (CurrentAnimationName != name)
            {
                return Play(name, 0);
            }
            return this;
        }

        public AnimationDisplay FollowedBy(string animationName)
        {
            NextAnimationQueue.Enqueue(animationName);
            return this;
        }

        internal void StopPlaying()
        {
            CurrentAnimationName = "";
            this.drawObject.Texture = null;
        }
    }
}