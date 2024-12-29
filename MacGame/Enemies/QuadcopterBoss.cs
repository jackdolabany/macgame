using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class QuadcopterBoss : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const int MaxHealth = 6;
        float explosionTimer = 0f;
        float dyingTimer = 0f;

        /// <summary>
        /// These are the attack phases for the quad.
        /// </summary>
        public enum AttackPhase
        {
            /// <summary>
            /// Flies around and drops bombs
            /// </summary>
            Phase1,

            /// <summary>
            /// Flies over you and tries to crash down on you.
            /// </summary>
            Phase2
        }
        
        public enum QuadState
        {
            Attacking,
            Dying,
            Dead
        }

        public QuadState state = QuadState.Attacking;

        private AttackPhase attackPhase
        {
            get
            {
                if (Health == 1 || Health == 2 || Health == 3)
                {
                    return AttackPhase.Phase2;
                }
                else
                {
                    return AttackPhase.Phase1;
                }
            }
        }

        public QuadcopterBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var flying = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 0), 2, "flying");
            flying.LoopAnimation = true;
            flying.Oscillate = true;
            flying.FrameLength = 0.05f;
            animations.Add(flying);

            animations.Play("flying");

            isEnemyTileColliding = false;
            Attack = 1;

            Health = MaxHealth;

            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            this.CollisionRectangle = new Rectangle(-28 * Game1.TileScale, -42 * Game1.TileScale, 55 * Game1.TileScale, 21 * Game1.TileScale);
        }

        public override void TakeHit(int damage, Vector2 force)
        {

            Health -= damage;

            SoundManager.PlaySound("ShootFromCannon", 0.6f, -0.2f);

            if (!IsTempInvincibleFromBeingHit)
            {
                InvincibleTimer += 2f;
            }

            if (Health <= 0)
            {
                // DEATH!!!
                state = QuadState.Dying;
                Dead = true;
                this.velocity = Vector2.Zero;
            }
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 40, Color.White, 120f);

            Enabled = false;
            base.Kill();
            
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            base.Update(gameTime, elapsed);

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            if (state == QuadState.Attacking)
            {
           

            }

            if (state == QuadState.Dying)
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

                dyingTimer += elapsed;
                if (dyingTimer >= 4f)
                {

                    this.Kill();
                    state = QuadState.Dead;
                 
                }
            }

            if (state == QuadState.Dead)
            {
                // Take them to wherever you need to take them. Once we figure out where that is.
            }

        }
    }
}