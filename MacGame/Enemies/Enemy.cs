using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;
using System.Collections.Generic;

namespace MacGame.Enemies
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

        public bool Alive
        {
            get
            {
                return !Dead;
            }
            set
            {
                Dead = !value;
            }
        }

        public List<Enemy> ExtraEnemiesToAddAfterConstructor = new List<Enemy>();

        /// <summary>
        /// Whether or not the player can hurt the enemy by jumping on them.
        /// </summary>
        public bool CanBeJumpedOn { get; protected set; } = true;

        /// <summary>
        /// Whether or not the player can kill the enemy with a shovel or apples.
        /// </summary>
        public bool CanBeHitWithWeapons { get; protected set; } = true;

        protected int Health { get; set; }
        public int Attack { get; set; } = 1;
        public bool IsCustomPlayerColliding { get; set; }
        protected Vector2 InitialWorldLocation { get; set; }

        /// <summary>
        /// some enemies may just shoot and have no need to check collisions with the player
        /// </summary>
        public bool IsPlayerColliding = true;

        public bool IsTempInvincibleFromBeingHit
        {
            get
            {
                return _invincibleTimer > 0;
            }
        }

        private float _invincibleTimer;
        public float InvincibleTimer
        {
            get { return _invincibleTimer; }
            set
            {
                _invincibleTimer = value;
            }
        }

        private float _invincibleFlashTimer = 0;
        protected float InvincibleTimeAfterBeingHit { get; set; } = 0.75f;

        public Vector2 GetPlayerDirection(Player player)
        {
            return GetDirectionTo(player);
        }

        public Vector2 GetDirectionTo(GameObject target)
        {
            var vect = target.WorldCenter - CollisionCenter;
            vect.Normalize();
            return vect;
        }

        public Player Player;

        protected Camera camera;

        public Enemy(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            isEnemyTileColliding = true;
            IsAbleToMoveOutsideOfWorld = false;
            Player = player;
            IsCustomPlayerColliding = false;
            this.camera = camera;
        }

        /// <summary>
        ///  return a vector of how hard to hit the player after contact.
        /// </summary>
        public virtual Vector2 GetHitBackBoost(Player player)
        {
            var hitBackBoost = new Vector2(100, -200);
            if (player.CollisionCenter.X < CollisionCenter.X)
            {
                hitBackBoost.X *= -1;
            }
            return hitBackBoost;
        }

        /// <summary>
        /// Waypoints are relative to the level upper left of the level and in units of Tiles. 
        /// </summary>
        public void GoToWaypoint(float speed, Waypoint wayPoint)
        {
            GoToLocation(speed, wayPoint.CenterLocation);
        }

        public void GoToLocation(float speed, Vector2 location)
        {
            var vectorToLocation = location - CollisionCenter;

            if (vectorToLocation.Length() <= 5f)
            {
                // you're already there.
                Velocity = Vector2.Zero;
                return;
            }

            vectorToLocation.Normalize();
            Velocity = speed * vectorToLocation;
        }

        /// <summary>
        /// Returns if you are within a tile of the waypoint. This may blow up if you are moving too fast.
        /// </summary>
        public bool IsAtWaypoint(Waypoint wayPoint)
        {
            var vectorToLocation = wayPoint.CenterLocation - CollisionCenter;

            return vectorToLocation.Length() <= 5f;
        }

        public virtual void PlayInvincibleHitSound()
        {

        }

        public virtual void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit || Dead || !Enabled)
            {
                return;
            }

            ForceVelocity += force;
            isEnemyTileColliding = false;
            Health -= damage;
            if (Health <= 0)
            {
                Kill();
            }
            else
            {
                PlayTakeHitSound();
                if (!IsTempInvincibleFromBeingHit)
                {
                    InvincibleTimer += InvincibleTimeAfterBeingHit;
                }
            }
        }

        public virtual void PlayTakeHitSound()
        {
            SoundManager.PlaySound("HarshHit");
        }

        public virtual void PlayDeathSound()
        {
            SoundManager.PlaySound("HitEnemy");
        }

        public virtual void Kill()
        {
            Dead = true;
            PlayDeathSound();
            Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (InvincibleTimer > 0)
            {

                _invincibleFlashTimer -= elapsed;

                if (_invincibleFlashTimer < 0)
                {
                    DisplayComponent.TintColor = Color.White * 0.4f;
                }
                else
                {
                    DisplayComponent.TintColor = Color.White;
                }
                if (_invincibleFlashTimer <= -0.1f)
                {
                    _invincibleFlashTimer = 0.1f;
                }

                InvincibleTimer -= elapsed;
                if (InvincibleTimer <= 0)
                {
                    InvincibleTimer = 0;
                }
            }
            else
            {
                DisplayComponent.TintColor = Color.White;
            }

            base.Update(gameTime, elapsed);

            if (Dead && onGround)
            {
                velocity = Vector2.Zero;
            }

            // decelerate the force from an impact
            var decelerateAmount = 2f;
            if (ForceVelocity != Vector2.Zero)
            {
                float newX = 0;
                float newY = 0;
                if (ForceVelocity.X > 0)
                {
                    newX = Math.Max(0f, ForceVelocity.X - decelerateAmount);
                }
                else
                {
                    newX = Math.Min(0f, ForceVelocity.X + decelerateAmount);
                }
                if (ForceVelocity.Y > 0)
                {
                    newY = Math.Max(0f, ForceVelocity.Y - decelerateAmount);
                }
                else
                {
                    newY = Math.Min(0f, ForceVelocity.Y + decelerateAmount);
                }

                // Prevent enemies from sticking on the ceiling
                if (onCeiling && newY < 0)
                {
                    newY = 0;
                }

                ForceVelocity = new Vector2(newX, newY);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (camera.IsObjectVisible(GetDrawRectangle()))
            {
                base.Draw(spriteBatch);
            }
        }

        public virtual void AfterHittingPlayer()
        {
            // Do nothing.
        }

        protected void AddEnemyInConstructor(Enemy enemy)
        {
            ExtraEnemiesToAddAfterConstructor.Add(enemy);
        }

        /// <summary>
        /// Override this to handle custom properties from object modifiers in the Tiled maps.
        /// </summary>
        public virtual void SetProps(Dictionary<string, string> props)
        {
            // Do nothing by default.
        }
    }
}
