using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

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

        private float _hitFlashTimer = 0f;
        private const float HitFlashDuration = 0.1f;

        public void TriggerHitFlash()
        {
            _hitFlashTimer = HitFlashDuration;
        }

        /// <summary>
        /// The default is to flash invisible for a moment when hit. Some enemies might want to
        /// override this. If enemies don't have a period of invincibility after being hit they
        /// won't flash.
        /// </summary>
        protected bool FlashesInvisibleWhenHit { get; set; } = true;

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

        protected Player Player;

        protected Camera camera;

        /// <summary>
        /// The current level's auto scroll speed.
        /// </summary>
        public Vector2 AutoScrollSpeed => Game1.CurrentLevel.AutoScrollSpeed;

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

        public bool IsOnScreen()
        {
            var isOnScreen = Game1.Camera.IsObjectVisible(this.CollisionRectangle, 1);
            return isOnScreen;
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

        protected bool CanTakeHit() => !IsTempInvincibleFromBeingHit && !Dead && Enabled;

        public virtual void TakeHit(GameObject attacker, int damage)
        {
            if (!CanTakeHit())
            {
                return;
            }

            isEnemyTileColliding = false;
            Health -= damage;
            if (Health <= 0)
            {
                Kill();
            }
            else
            {
                PlayTakeHitSound();
                InvincibleTimer += InvincibleTimeAfterBeingHit;
                TriggerHitFlash();
                _invincibleFlashTimer = 0.1f;
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

        protected void PlaySoundIfOnScreen(string soundName, float volume = 1f)
        {
            if (IsOnScreen())
            {
                SoundManager.PlaySound(soundName, volume);
            }
        }

        public virtual void Kill()
        {
            Dead = true;
            PlayDeathSound();
            Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (_hitFlashTimer > 0)
            {
                _hitFlashTimer -= elapsed;
                DisplayComponent.TintColor = Color.White;
                if (InvincibleTimer > 0)
                {
                    InvincibleTimer -= elapsed;
                    if (InvincibleTimer <= 0)
                    {
                        InvincibleTimer = 0;
                    }
                }
            }
            else if (InvincibleTimer > 0)
            {
                if (FlashesInvisibleWhenHit)
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

        }

        /// <summary>
        /// Some bosses or whatever set their draw depth behind the player when they die.
        /// </summary>
        public void SetDepthBehindPlayer()
        {
            var depth = MathHelper.Max(DrawDepth, Game1.Player.DrawDepth + 10 * Game1.MIN_DRAW_INCREMENT);
            SetDrawDepth(depth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsOnScreen())
            {
                if (_hitFlashTimer > 0)
                {
                    WhiteFlashManager.Register(this);
                }
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

        /// <summary>
        /// Checks a collection of collision rectangles that will damage the player or destroy his shots.
        /// </summary>
        protected void CheckExtraCollisionRectangles(IEnumerable<Rectangle> collisionRectangles)
        {
            foreach (var rect in collisionRectangles)
            {
                CheckExtraCollisionRectangle(rect);
            }
        }

        protected void CheckExtraCollisionRectangle(Rectangle rect)
        {
            if (rect.Intersects(Player.CollisionRectangle))
            {
                Player.TakeHit(this);
            }

            foreach (var shot in Player.Shots.RawList)
            {
                if (shot.Enabled && shot.CollisionRectangle.Intersects(rect))
                {
                    shot.Break();
                }
            }

            foreach (var bomb in Player.Bombs.RawList)
            {
                if (bomb.Enabled && bomb.CollisionRectangle.Intersects(rect))
                {
                    bomb.Break();
                }
            }
        }

        /// <summary>
        /// Use this to Draw an extra rectangles for debugging.
        /// </summary>
        protected void DrawExtraDebugRectangles(SpriteBatch spriteBatch, IEnumerable<Rectangle> rectangles, Color color)
        {
            if (Game1.DrawAllCollisionRects)
            { 
                foreach (var rect in rectangles)
                {
                    DrawExtraDebugRectangle(spriteBatch, rect, color);
                }
             }
        }

        /// <summary>
        /// Use this to Draw an extra collision rectangle or something for debugging.
        /// </summary>
        protected void DrawExtraDebugRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            if (Game1.DrawAllCollisionRects)
            {
                spriteBatch.Draw(Game1.TileTextures, rectangle, Game1.WhiteSourceRect, color);
            }
        }

        /// <summary>
        /// Sets the sock's CollectOrRevealAction to return the player to the map and door they came from.
        /// Call this in Initialize() for shooter level bosses. Or any other boss that takes you back where you came from.
        /// </summary>
        protected void SetSockReturnAction(MacGame.Items.Sock sock)
        {
            var mapName = Game1.PreviousMapName;
            var doorName = Game1.PreviousMapDoorName;
            if (!string.IsNullOrEmpty(mapName))
            {
                sock.CollectOrRevealAction = () =>
                {
                    GlobalEvents.FireDoorEntered(this, mapName, doorName, "", Game1.TransitionType.SlowFade);
                };
            }
        }
    }
}
