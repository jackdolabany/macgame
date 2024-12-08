using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public abstract class PickupObject : GameObject, IPickupObject
    {
        Vector2 originalWorldLocation;

        public bool IsPickedUp { get; private set; }
        protected Player _player;
        
        /// <summary>
        /// Track a short period after it's dropped so we can avoid colliding 
        /// with the player right away.
        /// </summary>
        float recentlyDroppedTimer;
        protected bool WasRecentlyDropped;


        public PickupObject(ContentManager content, int x, int y, Player player)
        {
            _player = player;
            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            Enabled = true;
            originalWorldLocation = WorldLocation;
            IsAffectedByGravity = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            // Fricton
            if (OnGround)
            {
                this.velocity.X -= (this.velocity.X * 2 * elapsed);
                if (Math.Abs(this.velocity.X) < 1f)
                {
                    this.velocity.X = 0;
                }
            }

            var velocityBeforeUpdate = this.velocity;

            if (IsPickedUp)
            {
                // No velocity and move to the player.
                this.Velocity = Vector2.Zero;
                this.WorldLocation = _player.WorldLocation + new Vector2(16 * (_player.Flipped ? -1 : 1), -8);
            }

            base.Update(gameTime, elapsed);

            // Bounce off walls.
            if ((OnLeftWall && velocityBeforeUpdate.X < 0) || (OnRightWall && velocityBeforeUpdate.X > 0))
            {
                // If you hit a wall travel in the opposite direction and reverse speed, lose some speed for momentum.
                this.velocity.X = velocityBeforeUpdate.X * 0.5f * -1f;
            }

            if (this.velocity != Vector2.Zero)
            {
                // if it's moving check for enemy collisions
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        enemy.TakeHit(1, this.Velocity);
                    }
                }
            }

            if (recentlyDroppedTimer > 0)
            {
                recentlyDroppedTimer -= elapsed;
                if (recentlyDroppedTimer <= 0)
                {
                    WasRecentlyDropped = false;
                }
            }
        }

        public void Pickup()
        {
            this.isTileColliding = false;
            this.IsAffectedByGravity = false;
            IsPickedUp = true;
        }

        public void Drop()
        {
            IsPickedUp = false;
            this.velocity = _player.Velocity;
            if (_player.IsFacingRight())
            {                 
                this.velocity.X += 50;
            }
            else
            {
                this.velocity.X += -50;
            }
            this.isTileColliding = true;
            this.MoveToIgnoreCollisions();
            this.IsAffectedByGravity = true;
            recentlyDroppedTimer = 0.5f;
            WasRecentlyDropped = true;
        }

        public void Kick()
        {
            this.Velocity = _player.Velocity + new Vector2(200 * (_player.IsFacingRight() ? 1 : -1), -200);
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);
            SoundManager.PlaySound("Jump");
        }

        public void MoveToPlayer()
        {
            this.WorldLocation = _player.WorldLocation + new Vector2(16 * (_player.Flipped ? -1 : 1), -8);
        }

        public bool CanBePickedUp
        {
            get
            {
                return Enabled;
            }
        }


    }

}
