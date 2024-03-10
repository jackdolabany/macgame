using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    public class FallingPlatform : Platform
    {
        private float fallCountDownTimer = 0.35f;

        Rectangle checkCollisionWithPlayerRect;

        bool timerStarted = false;

        public FallingPlatform(ContentManager content, int cellX, int cellY)
           : base(content, cellX, cellY)
        {
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures/Textures"), new Rectangle(0, 4*8, 8, 3));
            SetCenteredCollisionRectangle(16, 5);

            checkCollisionWithPlayerRect = new Rectangle(this.CollisionRectangle.X, this.CollisionRectangle.Y - 2, this.CollisionRectangle.Width, 2);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!timerStarted && Game1.Player.OnGround && checkCollisionWithPlayerRect.Intersects(Game1.Player.CollisionRectangle))
            {
                this.timerStarted = true;
            }

            if (timerStarted && !IsAffectedByGravity)
            {
                fallCountDownTimer -= elapsed;
                if (fallCountDownTimer <= 0)
                {
                    this.IsAffectedByGravity = true;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
