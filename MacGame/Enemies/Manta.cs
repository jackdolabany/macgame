using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class Manta : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 60;
        Vector2 startLocation;

        public MantaState State { get; set; }

        /// <summary>
        /// The Manta swims up for a bit, then pauses, then slowly floats
        /// back down to it's original location.
        /// </summary>
        public enum MantaState
        {
            SwimUp,
            IdleAtTop,
            FloatDown,
            IdleAtBottom
        }

        float idleTimer = 0;

        public Manta(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var swim = new AnimationStrip(textures, Helpers.GetTileRect(12, 7), 3, "swim");
            swim.LoopAnimation = true;
            swim.Oscillate = true;
            swim.FrameLength = 0.2f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetWorldLocationCollisionRectangle(6, 5);
            
            // Shift it up a bit.
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y - 12, this.collisionRectangle.Width, this.collisionRectangle.Height);

            this.startLocation = this.WorldLocation;

            State = MantaState.SwimUp;

        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            switch (this.State)
            {
                case MantaState.SwimUp:
                    this.velocity = new Vector2(0, -speed);
                    if (this.WorldLocation.Y < startLocation.Y - 100 || OnCeiling)
                    {
                        this.State = MantaState.IdleAtTop;
                        idleTimer = 2f;
                        this.Velocity = Vector2.Zero;
                    }
                    break;
                case MantaState.IdleAtTop:
                    idleTimer -= elapsed;

                    if (idleTimer <= 0)
                    {
                        this.State = MantaState.FloatDown;
                    }
                    break;
                case MantaState.FloatDown:
                    this.velocity.Y = speed / 2;

                    this.velocity.Y = MathHelper.Clamp(this.velocity.Y, -speed, speed);

                    if (this.WorldLocation.Y > startLocation.Y)
                    {
                        this.State = MantaState.IdleAtBottom;
                        idleTimer = 2f;
                        this.Velocity = Vector2.Zero;
                    }
                    break;
                case MantaState.IdleAtBottom:
                    idleTimer -= elapsed;
                    if (idleTimer <= 0)
                    {
                        this.State = MantaState.SwimUp;
                    }
                    break;
            }


            base.Update(gameTime, elapsed);

      

        }
    }
}