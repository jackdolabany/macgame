using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class Blowfish : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 60;
        const int MaxHealth = 20;

        public enum FishState
        {
            Attacking,
            Dying,
            Dead
        }

        FishState state = FishState.Attacking;

        float explosionTimer = 0f;
        float dyingTimer = 0f;
        float deathSqueaksTimer = 0f;


        /// <summary>
        /// After death reveal the sock.
        /// </summary>
        private Sock Sock;
        private bool _isInitialized = false;

        public enum SizeTarget
        {
            Small,
            Big
        }

        SizeTarget sizeTarget = SizeTarget.Small;

        // These timers control how long it takes to shrink or grow, but not the time in between.
        const float growTimerGoal = 0.5f;
        float growTimer = 0;

        // Shrink or grow after a while.
        float changeSizeTimer = 0;

        const float maxScale = 1f;
        const float minScale = 0.05f;

        Rectangle bigCollisionRectangle;
        Rectangle smallCollisionRectangle;

        List<Waypoint> waypoints = new List<Waypoint>();
        Waypoint nextWaypoint;

        public Blowfish(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var swim = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 2), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.3f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAbleToMoveOutsideOfWorld = true;
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            this.CollisionRectangle = new Rectangle(-50, -170, 100, 80);

            bigCollisionRectangle = this.collisionRectangle;
            smallCollisionRectangle = Rectangle.Empty;

            Scale = minScale;
            sizeTarget = SizeTarget.Small;
            growTimer = growTimerGoal;
        }

        /// <summary>
        /// Find anything we need that we expect to be in the map.
        /// </summary>
        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    Sock = (Sock)item;
                }
            }

            if (Sock == null)
            {
                throw new Exception("You need a sock in the level!");
            }

            Sock.Enabled = false;

            // Order the waypoints by distance from the frog and then distance to each other.
            var levelWaypoints = Game1.CurrentLevel.Waypoints.ToList();
            var pointToStartFrom = this.WorldLocation;
            while (levelWaypoints.Any())
            {
                var closestWaypoint = levelWaypoints.OrderBy(w => Vector2.Distance(w.Location, pointToStartFrom)).First();
                this.waypoints.Add(closestWaypoint);
                levelWaypoints.Remove(closestWaypoint);
                pointToStartFrom = closestWaypoint.Location;
            }
            nextWaypoint = waypoints.First();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }

            if (Scale > 0.9)
            {
                CollisionRectangle = bigCollisionRectangle;
            }
            else
            {
                CollisionRectangle = smallCollisionRectangle;
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            if (sizeTarget == SizeTarget.Small)
            {
                changeSizeTimer += elapsed;
                if (changeSizeTimer > 5f)
                {
                    Grow();
                }
            }

            if (sizeTarget == SizeTarget.Big)
            {
                changeSizeTimer += elapsed;
                if (changeSizeTimer > 10f)
                {
                    Shrink();
                }
            }

            // Shrink or grow based on growTimer
            if (sizeTarget == SizeTarget.Big)
            {
                if (growTimer < growTimerGoal)
                {
                    growTimer += elapsed;
                    Scale = MathHelper.Lerp(minScale, maxScale, growTimer / growTimerGoal);
                }
                else
                {
                    Scale = maxScale;
                }
            }
            else if (sizeTarget == SizeTarget.Small)
            {
                if (growTimer < growTimerGoal)
                {
                    growTimer += elapsed;
                    Scale = MathHelper.Lerp(maxScale, minScale, growTimer / growTimerGoal);
                }
                else
                {
                    Scale = minScale;
                }
            }

            GoToWaypoint(speed, nextWaypoint);

            if (IsAtWaypoint(nextWaypoint))
            {
                nextWaypoint = waypoints[(waypoints.IndexOf(nextWaypoint) + 1) % waypoints.Count];
            }

            // Change flipped if they're moving in a direction.
            // Maintain the value if velocity is 0 to prevent flickering.
            if (velocity.X < 0)
            {
                Flipped = true;
            }
            else if (velocity.X > 0)
            {
                Flipped = false;
            }

            base.Update(gameTime, elapsed);

            if (state == FishState.Dying)
            {

                // Add random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.25f)
                {
                    explosionTimer = 0f;

                    // Make explosions slightly larger than the collision rect
                    int explosionBuffer = 20;

                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(CollisionRectangle.Width + (explosionBuffer * 2));
                    var randomY = Game1.Randy.Next(CollisionRectangle.Height + (explosionBuffer * 2));

                    var randomLocation = new Vector2(CollisionRectangle.X + randomX - explosionBuffer, CollisionRectangle.Y + randomY - explosionBuffer);
                    EffectsManager.AddExplosion(randomLocation);
                }

                deathSqueaksTimer += elapsed;
                if (deathSqueaksTimer >= 0.65f)
                {
                    deathSqueaksTimer = 0f;
                    SoundManager.PlaySound("HitEnemy2");
                }

                dyingTimer += elapsed;
                if (dyingTimer >= 4f)
                {
                    this.Kill();
                    state = FishState.Dead;
                    Sock.FadeIn();
                }
            }

            if (state == FishState.Dead)
            {
                // Take them to wherever you need to take them. Once we figure out where that is.
            }
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit) return;

            Health -= damage;

            SoundManager.PlaySound("HitEnemy2");

            InvincibleTimer += 0.2f;

            if (Health <= 0)
            {
                // DEATH!!!
                state = FishState.Dying;
                Dead = true;
                this.velocity = Vector2.Zero;
            }
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public void Shrink()
        {
            growTimer = 0;
            changeSizeTimer = 0;
            sizeTarget = SizeTarget.Small;
            SoundManager.PlaySound("Shrink");
        }

        public void Grow()
        {
            growTimer = 0;
            changeSizeTimer = 0;
            sizeTarget = SizeTarget.Big;
            SoundManager.PlaySound("Grow");
        }

        public void ShootSpikes()
        {
            SoundManager.PlaySound("Shoot2");
        }
    }
}