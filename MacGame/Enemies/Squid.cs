using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Enemies
{
    public class Squid : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;

        private float idleTimer = 0f;
        private float moveTimer = 0f;
       
        public Squid(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var swim = new AnimationStrip(textures, Helpers.GetTileRect(14, 5), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.3f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 6);

            idleTimer = 1.5f;

        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            // Do nothing if off screen
            if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle)) return;

            if (idleTimer > 0)
            {
                idleTimer -= elapsed;
                if (idleTimer <= 0)
                {
                    moveTimer = 2.5f;
                    idleTimer = 0;
                }
            }
            else if (moveTimer > 0)
            {
                moveTimer -= elapsed;
                if (moveTimer <= 0)
                {
                    idleTimer = 2.5f;
                    moveTimer = 0;
                    this.Velocity = Vector2.Zero;
                }
                else if (Velocity == Vector2.Zero)
                {
                    // Pick an 8 way direction to move it.
                    if (Player.IsInWater)
                    {
                        // Move towards the player
                        var direction = Helpers.GetEightWayDirectionTowardsTarget(WorldCenter, Player.WorldCenter);
                        this.Velocity = direction * speed;
                    }
                    else
                    {
                        // If the player isn't in the water, move in a random direction like 
                        // you're not a threat.
                        this.Velocity = Game1.Randy.NextVector() * speed;
                    }
                    
                }
            }

            // If they start to get out of the water force their direction of movement downward.
            var tileAbove = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionCenter.X, this.CollisionRectangle.Top));
            if (tileAbove != null && !tileAbove.IsWater)
            {
                this.Velocity = new Vector2(this.Velocity.X, Math.Abs(this.Velocity.Y));
            }

            base.Update(gameTime, elapsed);

        }
    }
}