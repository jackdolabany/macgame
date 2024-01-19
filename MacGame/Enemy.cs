using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;
using System.Collections.Generic;

namespace MacGame
{
    public class Enemy : GameObject
    {

        private bool _dead = false;
        public bool Dead
        {
            get
            {
                return _dead || !Enabled;
            }
            set
            {
                _dead = value;
                if (_dead)
                {
                    isEnemyTileColliding = false;
                }
            }
        }

        public bool Alive { get { return !Dead; } }
        protected float Health { get; set; }
        public float Attack { get; set; }
        public bool IsCustomPlayerColliding { get; set; }
        protected Vector2 InitialWorldLocation { get; set; }

        /// <summary>
        /// some enemies may just shoot and have no need to check collisions with the player
        /// </summary>
        public bool IsPlayerColliding = true;

        public bool Invincible { get; set; }

        private float _invincibleTimer;
        public float InvincibleTimer
        {
            get { return _invincibleTimer; }
            set
            {
                _invincibleTimer = value;
                Invincible = InvincibleTimer > 0;
            }
        }

        public Vector2 GetPlayerDirection(Player player)
        {
            return GetDirectionTo(player);
        }

        public Vector2 GetDirectionTo(GameObject target)
        {
            var vect = target.WorldCenter - this.CollisionCenter;
            vect.Normalize();
            return vect;
        }

        public Player Player;

        protected Camera camera;

        public Enemy(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base()
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, cellY * TileMap.TileSize);
            Enabled = true;
            isEnemyTileColliding = true;
            IsAbleToMoveOutsideOfWorld = false;
            this.Player = player;
            IsCustomPlayerColliding = false;
            Invincible = false;
            this.camera = camera;
        }

        /// <summary>
        /// Used in the constructor to properly place enemies on the map.
        /// </summary>
        public virtual void InitializeOnMap()
        {
            //// Move them to the ground (the tile below).
            //var putOnGroundOffset = TileMap.TileSize - (this.collisionRectangle.Height + this.collisionRectangle.Y);
            //this.WorldLocation += new Vector2(0, putOnGroundOffset);
            //InitialWorldLocation = this.WorldLocation;
            //if (DisplayComponent is CharacterDisplay)
            //{
            //    // For these guys the world location is typically the center of their X positon.
            //    // Move them over half a tile so they are centered where you put them.
            //    this.worldLocation.X = this.worldLocation.X + (TileMap.TileWidth / 2);
            //}

        }

        /// <summary>
        /// Waypoints are relative to the level upper left of the level and in units of Tiles. 
        /// </summary>
        protected void GoToWaypoint(float speed, Vector2 wayPoint)
        {
            var currentTargetWorldLocation = (wayPoint * new Vector2(TileMap.TileSize, TileMap.TileSize)) + new Vector2(TileMap.TileSize / 2, TileMap.TileSize / 2);
            var vectorToLocation = currentTargetWorldLocation - CollisionCenter;
            vectorToLocation.Normalize();
            this.Velocity = speed * vectorToLocation;
        }

        /// <summary>
        /// Returns if you are within a tile of the waypoint. This may blow up if you are moving too fast.
        /// </summary>
        protected bool IsAtWaypoint(Vector2 wayPoint)
        {
            var center = this.CollisionCenter;
            var currentTarget = wayPoint * new Vector2(TileMap.TileSize, TileMap.TileSize);
            return (center.X > currentTarget.X
                && center.X < currentTarget.X + TileMap.TileSize
                && center.Y > currentTarget.Y
                && center.Y < currentTarget.Y + TileMap.TileSize);
        }

        public virtual void CollideWithPlayer()
        {
            //do nothing.
        }

        public virtual void HandleCustomPlayerCollision(Player player)
        {
            // do nothing   
        }

        public virtual void PlayInvincibleHitSound()
        {

        }

        public virtual void TakeHit(int damage, Vector2 force)
        {
            if (Invincible)
            {
                PlayInvincibleHitSound();
                return;
            }

            this.ForceVelocity += force;
            isEnemyTileColliding = false;
            Health -= damage;
            if (Health <= 0)
            {
                Kill();
                PlayDeathSound();
            }
            else
            {
                PlayTakeHitSound();
            }
        }

        public virtual void PlayTakeHitSound()
        {
            // TODO
        }

        public virtual void PlayDeathSound()
        {
            // TODO
        }

        public virtual void Kill()
        {
            Dead = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (InvincibleTimer > 0)
            {
                InvincibleTimer -= elapsed;
                if (InvincibleTimer <= 0)
                {
                    InvincibleTimer = 0;
                    Invincible = false;
                }
            }

            base.Update(gameTime, elapsed);

            if (Dead && onGround)
            {
                velocity = Vector2.Zero;
            }

            // decelerate the force from an impact
            var decelerateAmount = 2f;
            if (this.ForceVelocity != Vector2.Zero)
            {
                float newX = 0;
                float newY = 0;
                if (this.ForceVelocity.X > 0)
                {
                    newX = Math.Max(0f, this.ForceVelocity.X - decelerateAmount);
                }
                else
                {
                    newX = Math.Min(0f, this.ForceVelocity.X + decelerateAmount);
                }
                if (this.ForceVelocity.Y > 0)
                {
                    newY = Math.Max(0f, this.ForceVelocity.Y - decelerateAmount);
                }
                else
                {
                    newY = Math.Min(0f, this.ForceVelocity.Y + decelerateAmount);
                }

                // Prevent enemies from sticking on the ceiling
                if (onCeiling && newY < 0)
                {
                    newY = 0;
                }

                this.ForceVelocity = new Vector2(newX, newY);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (camera.IsObjectVisible(GetDrawRectangle()))
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
