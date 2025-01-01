using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class CatBoss : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        bool hasBeenSeen = false;

        YarnBall[] yarnBalls = new YarnBall[3];

        int nextYarnBallToThrowIndex = 0;
        const float maxThrowTimer = 2f;
        float throwTimer = maxThrowTimer;

        int walkSpeed = 120;
        int maxTravelDistance = 24;
        int startLocationX;
        const int MaxHealth = 6;
        float jumpTimer = 0f;
        float explosionTimer = 0f;
        float deathSqueaksTimer = 0f;

        float dyingTimer = 0f;

        /// <summary>
        /// These are the attack phases for the cat.
        /// </summary>
        public enum AttackPhase
        {
            /// <summary>
            /// Phase 1 walk around and shoot random balls.
            /// </summary>
            Phase1,

            /// <summary>
            /// Now the balls bounce off the room.
            /// </summary>
            Phase2,

            /// <summary>
            /// Same as phase 2 but the cat jumps randomly now.
            /// </summary>
            Phase3
        }
        
        public enum CatState
        {
            Attacking,
            Dying,
            Dead
        }

        public CatState state = CatState.Attacking;

        private AttackPhase attackPhase
        {
            get
            {
                if (Health == 1 || Health == 2)
                {
                    return AttackPhase.Phase3;
                }
                else if (Health == 3 || Health == 4)
                {
                    return AttackPhase.Phase2;
                }
                else
                {
                    return AttackPhase.Phase1;
                }
            }
        }

        public CatBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 0), 3, "idle");
            idle.LoopAnimation = true;
            idle.Oscillate = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;

            Health = MaxHealth;

            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetCenteredCollisionRectangle(14, 14);

            // Cat has yarn balls.
            for (int i = 0; i < yarnBalls.Length; i++)
            {
                yarnBalls[i] = new YarnBall(content, 0, 0, player, camera);
                yarnBalls[i].Enabled = false;
                Level.AddEnemy(yarnBalls[i]);
            }
            startLocationX = WorldLocation.X.ToInt();
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            var previousPhase = attackPhase;

            Health -= damage;

            SoundManager.PlaySound("CatBossHit");

            if (!IsTempInvincibleFromBeingHit)
            {
                InvincibleTimer += 2f;
            }

            if (Alive && previousPhase == AttackPhase.Phase2 && attackPhase == AttackPhase.Phase3)
            {
                // Kill the yarn balls between attack phase 2 and 3.
                foreach (var yarnball in yarnBalls)
                {
                    yarnball.Kill();
                }
            }

            if (Health <= 0)
            {
                // DEATH!!!
                state = CatState.Dying;
                Dead = true;
                this.velocity = Vector2.Zero;
                foreach (var yarnball in yarnBalls)
                {
                    yarnball.Kill();
                }
            }
        }

        public override void Kill()
        {
            Enabled = false;
            base.Kill();

            // TODO: Final boss, just for now.
            TimerManager.AddNewTimer(2f, () =>
            {
                GlobalEvents.FireFinalBossComplete();
            });
            
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            base.Update(gameTime, elapsed);

            if (!hasBeenSeen)
            {
                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    hasBeenSeen = true;
                    SoundManager.PlaySong("BossFight", true, 0.2f);
                }
            }

            if (!hasBeenSeen) return;

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            if (state == CatState.Attacking)
            {
                throwTimer -= elapsed;
                if (throwTimer < 0f)
                {
                    YarnBall? availableYarnBall = null;

                    for (int i = 0; i < yarnBalls.Length; i++)
                    {
                        if (!yarnBalls[i].Alive)
                        {
                            availableYarnBall = yarnBalls[i];
                            break;
                        }
                    }

                    if (availableYarnBall != null)
                    {
                        availableYarnBall.Enabled = true;
                        availableYarnBall.Alive = true;
                        availableYarnBall.WorldLocation = WorldCenter;
                        var direction = Player.WorldCenter - availableYarnBall.WorldCenter;
                        direction.Normalize();
                        availableYarnBall.Velocity = direction * 200;

                        SoundManager.PlaySound("CatBossShoot");

                        // Balls start bouncing later.
                        availableYarnBall.IsBouncing = attackPhase == AttackPhase.Phase2 || attackPhase == AttackPhase.Phase3;
                    }

                    throwTimer = maxThrowTimer;
                }

                velocity.X = walkSpeed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }

                if (OnLeftWall)
                {
                    Flipped = false;
                }
                else if (OnRightWall)
                {
                    Flipped = true;
                }

                // Jump in phase 3
                if (attackPhase == AttackPhase.Phase3)
                {
                    if (OnGround)
                    {
                        jumpTimer += elapsed;
                        if (jumpTimer >=  0.8f)
                        {
                            jumpTimer = 0f;
                            velocity.Y = -600;
                            SoundManager.PlaySound("CatBossJump");
                        }
                    }
                }

            }

            if (state == CatState.Dying)
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
                    SoundManager.PlaySound("CatBossHit");
                }

                dyingTimer += elapsed;
                if (dyingTimer >= 4f)
                {

                    this.Kill();
                    state = CatState.Dead;
                 
                }
            }

            if (state == CatState.Dead)
            {
                // Take them to wherever you need to take them. Once we figure out where that is.
            }

        }
    }
}