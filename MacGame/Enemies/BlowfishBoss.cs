using System;
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

        private float speed = 40;
        private float maxTravelDistance = 5 * Game1.TileSize;

        private float minXLocation;
        private float maxXLocation;
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
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            this.CollisionRectangle = new Rectangle(-50, -170, 100, 80);

            var startLocationX = WorldLocation.X;
            minXLocation = startLocationX - maxTravelDistance / 2;
            maxXLocation = startLocationX + maxTravelDistance / 2;
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
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }


            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            if (Alive)
            {
                velocity.X = speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            if (velocity.X > 0 && (WorldLocation.X >= maxXLocation || OnRightWall))
            {
                Flipped = !Flipped;
                minXLocation = WorldLocation.X - maxTravelDistance;
            }
            else if (velocity.X < 0 && (WorldLocation.X <= minXLocation || OnLeftWall))
            {
                Flipped = !Flipped;
                maxXLocation = WorldLocation.X + maxTravelDistance;
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
    }
}