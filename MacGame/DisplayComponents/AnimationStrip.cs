using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.DisplayComponents
{
    public class AnimationStrip
    {
        private float frameTimer = 0f;

        public int currentFrameIndex;

        Rectangle FirstFrame { get; set; }

        public int FrameWidth
        {
            get { return FirstFrame.Width; }
        }

        public int FrameHeight
        {
            get { return FirstFrame.Height; }
        }

        public Texture2D Texture { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Add another animation if you want it to play after this one immediately.
        /// </summary>
        public string NextAnimation { get; set; }

        public bool LoopAnimation { get; set; }

        /// <summary>
        /// If true the animation will start working backwards once it gets to the end. Otherwise it will 
        /// start back at the beginning.
        /// </summary>
        public bool Oscillate { get; set; }

        public bool FinishedPlaying { get; private set; }

        public int FrameCount { get; set; }

        /// <summary>
        /// A length of time to display each frame.
        /// </summary>
        public float FrameLength { get; set; }

        public bool Reverse { get; set; }

        /// <summary>
        /// Use this to pause animations. Like a climbing animation that pauses if you aren't moving.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// The current frame rectangle relative to the texture.
        /// </summary>
        public Rectangle FrameRectangle
        {
            get
            {

                int realCurrentFrameIndex = currentFrameIndex;
                if (Reverse)
                {
                    realCurrentFrameIndex = FrameCount - 1 - currentFrameIndex;
                }

                int x = FirstFrame.X + (realCurrentFrameIndex * FrameWidth);
                int y = FirstFrame.Y;

                // Add padding to deal with the 2 pixel border between tiles.
                return new Rectangle(
                    x + (2 * realCurrentFrameIndex),
                    y,
                    FrameWidth,
                    FrameHeight);
            }
        }

        /// <summary>
        /// Creates a 2D animation. We expect a texture with frames moving to the right.
        /// </summary>
        /// <param name="firstFrame">A rectangle relative to the texture of the first frame. All other frames are expected to be to the right.</param>
        /// <param name="frameCount">The number of frames in the animation.</param>
        public AnimationStrip(Texture2D texture, Rectangle firstFrame, int frameCount, string name)
        {
            Texture = texture;
            FirstFrame = firstFrame;
            FrameCount = frameCount;
            Name = name;
            FrameLength = 0.05f;
            NextAnimation = "";
        }

        public AnimationStrip Play(int currentFrame)
        {
            currentFrameIndex = currentFrame;
            frameTimer = 0;
            FinishedPlaying = false;
            return this;
        }

        public AnimationStrip Play()
        {
            return Play(0);
        }

        public AnimationStrip FollowedBy(string animationName)
        {
            NextAnimation = animationName;
            return this;
        }

        public void Update(float elapsed)
        {
            if (IsPaused) return;

            frameTimer += elapsed;

            if (frameTimer >= FrameLength)
            {
                currentFrameIndex++;
                if (currentFrameIndex >= FrameCount)
                {
                    if (LoopAnimation)
                    {
                        currentFrameIndex = 0;
                        if (Oscillate)
                        {
                            Reverse = !Reverse;
                        }
                    }
                    else
                    {
                        currentFrameIndex = FrameCount - 1;
                        FinishedPlaying = true;
                    }
                }

                frameTimer = 0f;
            }
        }

        public object Clone()
        {
            AnimationStrip clone = new AnimationStrip(Texture, FirstFrame, FrameCount, Name);
            clone.currentFrameIndex = currentFrameIndex;
            clone.FrameLength = FrameLength;
            clone.LoopAnimation = LoopAnimation;
            clone.NextAnimation = NextAnimation;
            clone.Oscillate = Oscillate;
            clone.Reverse = Reverse;
            return clone;
        }
    }
}
