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

        public bool IsPickedUp { get; private set; }
        protected Player _player;

        public virtual float Friction
        {
            get
            {
                return 3.5f;
            }
        }

        public PickupObject(ContentManager content, int x, int y, Player player)
        {
            _player = player;
            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            Enabled = true;
            IsAffectedByGravity = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            // Fricton
            if (OnGround)
            {
                this.velocity.X -= (this.velocity.X * Friction * elapsed);
                if (Math.Abs(this.velocity.X) < 15f)
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

            var wasOnGround = OnGround;
            var previousLocation = this.worldLocation;

            base.Update(gameTime, elapsed);

            // If you fell and landed on the ground.
            if (onGround && !wasOnGround && this.worldLocation.Y > previousLocation.Y)
            {
                SoundManager.PlaySound("Bounce");
            }

            // Bounce off walls.
            if ((OnLeftWall && velocityBeforeUpdate.X < 0) || (OnRightWall && velocityBeforeUpdate.X > 0))
            {
                // If you hit a wall travel in the opposite direction and reverse speed, lose some speed for momentum.
                this.velocity.X = velocityBeforeUpdate.X * 0.5f * -1f;
                SoundManager.PlaySound("Bounce");
            }

            if (this.velocity != Vector2.Zero)
            {
                // if it's moving check for enemy collisions
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.Enabled && enemy.Alive && enemy.CanBeHitWithWeapons && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        enemy.TakeHit(this, 1, this.Velocity);
                    }
                }
            }
        }

        public void Pickup()
        {
            this.isTileColliding = false;
            this.IsAffectedByGravity = false;
            IsPickedUp = true;
            SoundManager.PlaySound("Pickup");
        }

        public virtual void Drop()
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
        }

        public void Kick(bool isStraightUp)
        {
            if (isStraightUp)
            {
                this.Velocity = _player.Velocity + new Vector2(0, -600);
            }
            else
            {
                this.Velocity = _player.Velocity + new Vector2(200 * (_player.IsFacingRight() ? 1 : -1), -200);
            }
            EffectsManager.SmallEnemyPop(WorldCenter);
            SoundManager.PlaySound("Kick");
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
