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

        private float speed = 150;
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

        /// <summary>
        /// Don't move until you are seen.
        /// </summary>
        private bool _wasSeen = false;

        public enum SizeTarget
        {
            Small,
            Big
        }

        SizeTarget sizeTarget = SizeTarget.Small;

        /// <summary>
        /// How long it takes to shrink or grow.
        /// </summary>
        const float growTimerGoal = 0.3f;
        float growTimer = 0;

        // Shrink or grow after a while.
        float changeSizeTimer = 0;

        /// <summary>
        /// Scale size when big
        /// </summary>
        const float maxScale = 1f;

        /// <summary>
        /// Scale size when small.
        /// </summary>
        const float minScale = 0.1f;

        Rectangle bigCollisionRectangle;
        Rectangle smallCollisionRectangle;

        List<Waypoint> waypoints = new List<Waypoint>();
        Waypoint nextWaypoint;

        CircularBuffer<BlowfishSpike> spikes;
        float shootdelayTimer = 0f;

        /// <summary>
        /// Wait this long before shooting once big.
        /// </summary>
        const float shootDelayTimerGoal = 4f;

        float betweenShotsDelayTimer = 0f;

        /// <summary>
        /// Take this much time between shots
        /// </summary>
        const float betweenShotsDelayTimerGoal = 0.75f;

        AnimationDisplay bigFishAnimationDisplay;
        AnimationDisplay smallFishAnimationDisplay;

        // Track the draw depth of the fish and just behind him.
        // we'll swap the big and small fish in front of each other as the fish
        // shrinks and grows.
        float frontDrawDepth;
        float backDrawDepth;

        /// <summary>
        /// Fish starts small for a while, track if he grew so that we don't have to 
        /// wait as long to change size next time.
        /// </summary>
        bool _hasGrown = false;

        public Blowfish(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            bigFishAnimationDisplay = new AnimationDisplay();
            smallFishAnimationDisplay = new AnimationDisplay();

            DisplayComponent = new AggregateDisplay(new List<DisplayComponent> { bigFishAnimationDisplay, smallFishAnimationDisplay });

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var smallSwim = new AnimationStrip(textures, Helpers.GetTileRect(10, 26), 2, "swim");
            smallSwim.LoopAnimation = true;
            smallSwim.FrameLength = 0.3f;
            smallFishAnimationDisplay.Add(smallSwim);
            smallFishAnimationDisplay.Play("swim");

            var megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var bigSwim = new AnimationStrip(megaTextures, Helpers.GetMegaTileRect(0, 2), 2, "swim");
            bigSwim.LoopAnimation = true;
            bigSwim.FrameLength = 0.3f;
            bigFishAnimationDisplay.Add(bigSwim);
            bigFishAnimationDisplay.Play("swim");

            // Line them up so the small fish is in the center of the big one.
            bigFishAnimationDisplay.RotationAndDrawOrigin = new Vector2(0, 0);
            smallFishAnimationDisplay.RotationAndDrawOrigin = new Vector2(0, 30 * Game1.TileScale);

            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAbleToMoveOutsideOfWorld = true;
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            this.CollisionRectangle = new Rectangle(-50, -170, 100, 80);

            bigCollisionRectangle = this.collisionRectangle;
            smallCollisionRectangle = new Rectangle(-8, -132, 5 * Game1.TileScale, 4 * Game1.TileScale);

            bigFishAnimationDisplay.Scale = minScale;
            sizeTarget = SizeTarget.Small;
            growTimer = growTimerGoal;

            spikes = new CircularBuffer<BlowfishSpike>(64);
            for (int i = 0; i < spikes.Length; i++)
            {
                var spike = new BlowfishSpike(content, 0, 0, player, camera);
                spikes.SetItem(i, spike);
            }

            this.ExtraEnemiesToAddAfterConstructor.AddRange(spikes);
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
                var closestWaypoint = levelWaypoints.OrderBy(w => Vector2.Distance(w.CenterLocation, pointToStartFrom)).First();
                this.waypoints.Add(closestWaypoint);
                levelWaypoints.Remove(closestWaypoint);
                pointToStartFrom = closestWaypoint.CenterLocation;
            }
            nextWaypoint = waypoints.First();

            frontDrawDepth = this.DrawDepth;
            backDrawDepth = frontDrawDepth + Game1.MIN_DRAW_INCREMENT;

            smallFishAnimationDisplay.DrawDepth = frontDrawDepth;
            bigFishAnimationDisplay.DrawDepth = backDrawDepth;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }

            // Don't do anything until you are seen.
            if (!_wasSeen && camera.IsPointVisible(this.CollisionCenter))
            {
                _wasSeen = true;
            }

            if (!_wasSeen)
            {
                return;
            }

            if (bigFishAnimationDisplay.Scale < 0.3f)
            {
                smallFishAnimationDisplay.DrawDepth = frontDrawDepth;
                bigFishAnimationDisplay.DrawDepth = backDrawDepth;
            }
            else
            {
                smallFishAnimationDisplay.DrawDepth = backDrawDepth;
                bigFishAnimationDisplay.DrawDepth = frontDrawDepth;
            }

            if (bigFishAnimationDisplay.Scale > 0.9)
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
            Game1.BossName = "Puff";

            if (state == FishState.Attacking)
            {

                if (sizeTarget == SizeTarget.Small)
                {
                    if (Health < MaxHealth) // If they've been hit, they'll grow after a while.
                    {
                        var growGoal = _hasGrown ? 5f : 10f;
                        changeSizeTimer += elapsed;
                        if (changeSizeTimer > growGoal)
                        {
                            Grow();
                        }
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
                        bigFishAnimationDisplay.Scale = MathHelper.Lerp(minScale, maxScale, growTimer / growTimerGoal);
                    }
                    else
                    {
                        bigFishAnimationDisplay.Scale = maxScale;
                    }
                }
                else if (sizeTarget == SizeTarget.Small)
                {
                    if (growTimer < growTimerGoal)
                    {
                        growTimer += elapsed;
                        bigFishAnimationDisplay.Scale = MathHelper.Lerp(maxScale, minScale, growTimer / growTimerGoal);
                    }
                    else
                    {
                        bigFishAnimationDisplay.Scale = minScale;
                    }
                }

                GoToWaypoint(speed, nextWaypoint);

                if (IsAtWaypoint(nextWaypoint))
                {
                    nextWaypoint = waypoints[(waypoints.IndexOf(nextWaypoint) + 1) % waypoints.Count];
                }

                if (sizeTarget == SizeTarget.Big && shootdelayTimer < shootDelayTimerGoal)
                {
                    shootdelayTimer += elapsed;
                }
                else if (sizeTarget == SizeTarget.Big && shootdelayTimer >= shootDelayTimerGoal)
                {
                    betweenShotsDelayTimer += elapsed;
                    if (betweenShotsDelayTimer >= betweenShotsDelayTimerGoal)
                    {
                        ShootSpikes();
                        betweenShotsDelayTimer = 0f;
                    }
                }
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

                    // break these bricks so that Mac can leave.
                    Game1.CurrentLevel.BreakBricks("Puff");
                }
            }

            if (state == FishState.Dead)
            {
                // Take them to wherever you need to take them. Once we figure out where that is.
            }

            //bigFishAnimationDisplay.Scale = 0.1f;
            //bigFishAnimationDisplay.DrawDepth = frontDrawDepth;
            //smallFishAnimationDisplay.DrawDepth = backDrawDepth;

        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit) return;

            if (Health == MaxHealth)
            {
                // Grow on his first hit
                Grow();
            }

            Health -= damage;

            SoundManager.PlaySound("HitEnemy2");

            InvincibleTimer += 0.2f;

            if (Health <= 0)
            {
                // DEATH!!!
                state = FishState.Dying;
                Dead = true;
                this.velocity = Vector2.Zero;

                foreach (BlowfishSpike spike in spikes)
                {
                    spike.Attack = 0;
                }
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
            shootdelayTimer = 0f;
        }

        public void Grow()
        {
            growTimer = 0;
            changeSizeTimer = 0;
            sizeTarget = SizeTarget.Big;
            SoundManager.PlaySound("Grow");
            shootdelayTimer = 0f;
            _hasGrown = true;
        }

        public void ShootSpikes()
        {
            SoundManager.PlaySound("Shoot2");
            for (int i = 0; i < 8; i++)
            {
                var spike = spikes.GetNextObject();
                spike.Enabled = true;
                spike.WorldLocation = this.CollisionCenter;
                var direction = (EightWayRotationDirection)i;
                spike.RotationDirection = new EightWayRotation(direction);
                spike.Velocity = spike.RotationDirection.Vector2 * 200;
                spike.SetDrawDepth(this.DrawDepth + (Game1.MIN_DRAW_INCREMENT * 2));
            }
        }
    }
}