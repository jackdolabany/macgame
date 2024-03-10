using Microsoft.Xna.Framework;
using TileEngine;
using Microsoft.Xna.Framework.Graphics;
using MacGame.DisplayComponents;

namespace MacGame
{
    public class Particle : GameObject
    {
        private Vector2 acceleration;
        private float maxSpeed;
        private int initialDuration;
        private int remainingDuration;
        private Color initialColor;
        private Color finalColor;

        public int ElapsedDuration
        {
            get
            {
                return initialDuration - remainingDuration;
            }
        }

        /// <summary>
        /// If your particle has a static image display component,
        /// this is a shorthand way to change the image. This is
        /// useful when trying to switch up object pooled particles.
        /// </summary>
        public void SetStaticImage(Texture2D texture, Rectangle source)
        {
            ((StaticImageDisplay)this.DisplayComponent).DrawObject.Texture = texture;
            ((StaticImageDisplay)this.DisplayComponent).DrawObject.SourceRectangle = source;
            this.RotationAndDrawOrigin = source.RelativeCenterVector();
        }

        public float InitialScale { get; set; }
        public float FinalScale { get; set; }

        public bool CanSurviveOffScreen = false;

        public float DurationProgress
        {
            get
            {
                return (float)ElapsedDuration /
                    (float)initialDuration;
            }
        }

        public bool IsActive
        {
            get
            {
                return (remainingDuration > 0);
            }
        }

        public Particle(
            Vector2 location,
            Vector2 velocity,
            Vector2 acceleration,
            float maxSpeed,
            int duration,
            Color initialColor,
            Color finalColor)
        {
            Initialize(location, velocity, acceleration, maxSpeed, duration, initialColor, finalColor);
        }

        public void Initialize(
            Vector2 location,
            Vector2 velocity,
            Vector2 acceleration,
            float maxSpeed,
            int duration,
            Color initialColor,
            Color finalColor)
        {
            isTileColliding = false;
            this.worldLocation = location;
            this.velocity = velocity;
            initialDuration = duration;
            remainingDuration = duration;
            this.acceleration = acceleration;
            this.initialColor = initialColor;
            this.IsAffectedByGravity = false;
            this.maxSpeed = maxSpeed;
            this.finalColor = finalColor;
            this.Enabled = true;
            this.isEnemyTileColliding = false;
            this.CanSurviveOffScreen = true;
            IsRotationClockwise = true;
            RotationsPerSecond = 0;
            this.InitialScale = 1f;
            this.FinalScale = 1f;
            this.Enabled = true;
            this.flipped = false;
            this.collisionRectangle = new Rectangle();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (remainingDuration <= 0)
            {
                Enabled = false;
            }

            if (!CanSurviveOffScreen)
            {
                //if the particle is offscreen, kill it!
                if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    this.Enabled = false;
                }
            }

            if (Enabled)
            {
                this.velocity += acceleration;
                if (velocity.Length() > maxSpeed)
                {
                    Vector2 vel = velocity;
                    vel.Normalize();
                    velocity = vel * maxSpeed;
                }

                DisplayComponent.TintColor = Color.Lerp(
                    initialColor,
                    finalColor,
                    DurationProgress);
                remainingDuration--;

                this.DisplayComponent.Scale = (InitialScale * (1 - DurationProgress)) + (FinalScale * DurationProgress);

            }

            base.Update(gameTime, elapsed);
        }

        public new float Scale
        {
            get { return this.DisplayComponent.Scale; }
            set
            {
                this.DisplayComponent.Scale = value;
                this.InitialScale = value;
                this.FinalScale = value;
            }
        }
    }
}
