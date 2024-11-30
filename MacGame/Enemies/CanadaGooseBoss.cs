using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class CanadaGooseBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private int _honkCount = 0;

        public enum GooseState
        {
            InitialHonks,
            Idle,
            Attacking,
            Dying,
            Dead
        }
        private GooseState state = GooseState.Idle;

        float idleTimer = 0;
        float explosionTimer = 0;

        public List<Enemy> GooseBalls = new List<Enemy>();
        public Vector2 idleHeadLocation;

        public int previousFrameIndex = 0;

        public CanadaGooseBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            var honk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "honk");
            honk.LoopAnimation = false;
            honk.FrameLength = 0.14f;
            animations.Add(honk);

            var repeatHonk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "repeatHonk");
            repeatHonk.LoopAnimation = true;
            repeatHonk.FrameLength = 0.14f;
            animations.Add(repeatHonk);

            var attack = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 1), 3, "attack");
            attack.LoopAnimation = false;
            attack.FrameLength = 0.14f;
            attack.Oscillate = true;
            animations.Add(attack);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 3;
            IsAffectedByGravity = false;

            collisionRectangle = new Rectangle(-120, -200, 150, 200);

            state = GooseState.InitialHonks;

            TimerManager.AddNewTimer(1.5f, () =>
            {
                animations.Play("honk").FollowedBy("idle");
            });
            TimerManager.AddNewTimer(2.2f, () =>
            {
                animations.Play("honk").FollowedBy("idle");
            });
            TimerManager.AddNewTimer(2.9f, () =>
            {
                state = GooseState.Idle;
            });

            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));
            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));

            //new Vector2(32, 20);


            idleHeadLocation = worldLocation + new Vector2(16, -176);
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (state == GooseState.Idle)
            {
                idleTimer += elapsed;

                if (idleTimer >= 1.5f)
                {
                    state = GooseState.Attacking;
                    idleTimer = 0;
                }
            }
            if (state == GooseState.Attacking)
            {
                if (animations.CurrentAnimationName == "idle")
                {
                    if (Game1.Randy.NextBool())
                    {
                        animations.Play("honk");
                    }
                    else
                    {
                        animations.Play("attack");
                    }
                }
                else if (animations.CurrentAnimationName == "honk" && animations.CurrentAnimation!.currentFrameIndex == 1 && previousFrameIndex == 0)
                {
                    // When we honk shoot an idle ball when the frame swaps from 0 to 1.
                    foreach (var ball in GooseBalls)
                    {
                        if (!ball.Enabled || ball.Dead)
                        {
                            ball.Enabled = true;
                            ball.Dead = false;

                            var yVelocity = -Game1.Randy.Next(0, 400);
                            ball.Velocity = new Vector2(0, yVelocity);

                            ball.WorldLocation = idleHeadLocation + new Vector2(0, 16);

                            break;
                        }
                    }
                }
                else
                {
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        animations.Play("idle");
                        state = GooseState.Idle;
                    }
                }
            }

            if (state == GooseState.Dying)
            {
                // Honk like crazy
                this.animations.PlayIfNotAlreadyPlaying("repeatHonk");
                
                // random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.2f)
                {
                    explosionTimer = 0f;
                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(CollisionRectangle.Width);
                    var randomY = Game1.Randy.Next(CollisionRectangle.Height);

                    var randomLocation = new Vector2(CollisionRectangle.X + randomX, CollisionRectangle.Y + randomY);
                    EffectsManager.AddExplosion(randomLocation);
                }

                // TODO: Count down a timer and then just be dead.
            }
         
            foreach (var ball in GooseBalls)
            {
                if (ball.Enabled)
                {
                    ball.Update(gameTime, elapsed);
                }
            }

            previousFrameIndex = animations.CurrentAnimation!.currentFrameIndex;

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            // Draw the balls
            foreach (var ball in GooseBalls)
            {
                if (ball.Enabled && ball.Alive)
                {
                    ball.Draw(spriteBatch);
                }
            }

            base.Draw(spriteBatch);
        }
    }
}