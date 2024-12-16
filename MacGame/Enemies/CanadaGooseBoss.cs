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

        // the goose will stretch his head out across the screen and the necks will fill in behind.
        public CanadaGooseHead Head;
        public List<CanadaGooseNeck> Necks;
        Vector2 initialHeadLocation;

        Rectangle regularCollisionRectangle;
        Rectangle duckedDownCollisionRectangle;

        private Player _player;

        public CanadaGooseBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

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

            var attack = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 1), 2, "attack");
            attack.LoopAnimation = false;
            attack.FrameLength = 0.14f;
            animations.Add(attack);

            var attackUp = (AnimationStrip)attack.Clone();
            attackUp.Name = "attackUp";
            attackUp.Reverse = true;
            animations.Add(attackUp);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 3;
            IsAffectedByGravity = false;

            regularCollisionRectangle = new Rectangle(-120, -200, 150, 200);
            duckedDownCollisionRectangle = new Rectangle(-120, -100, 150, 100);
            collisionRectangle = regularCollisionRectangle;

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

            Head = new CanadaGooseHead(content, cellX, cellY, player, camera);
            Necks = new List<CanadaGooseNeck>();
            for (int i = 0; i < 10; i++)
            {
                Necks.Add(new CanadaGooseNeck(content, cellX, cellY, player, camera));
            }

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
            // This frame of animation has no head.
            Head.Enabled = animations.CurrentAnimationName == "attack" && animations.CurrentAnimation.currentFrameIndex == 1
                || animations.CurrentAnimationName == "attackUp" && animations.CurrentAnimation.currentFrameIndex == 0;


            if (Head.Enabled)
            {
                // Fill in between the head and body with glorious necks
                var gapRemaining = Head.WorldLocation.X - initialHeadLocation.X + 8;
                var nextNeckPosition = initialHeadLocation + new Vector2(-16, -16);
                if (gapRemaining > 0)
                {
                    foreach (var neck in Necks)
                    {
                        if (gapRemaining > 0)
                        {
                            neck.WorldLocation = nextNeckPosition;
                            nextNeckPosition.X += Game1.TileSize;
                            gapRemaining -= Game1.TileSize;
                            neck.Alive = true;
                            neck.Enabled = true;
                            neck.SetDrawDepth(Head.DrawDepth + (Game1.MIN_DRAW_INCREMENT * 10));
                        }
                        else
                        {
                            neck.Enabled = false;
                        }

                    }
                }
            }
            else
            {
                foreach (var neck in Necks)
                {
                    neck.Enabled = false;
                }
            }

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
                else if (animations.CurrentAnimationName == "attack")
                {
                    CollisionRectangle = duckedDownCollisionRectangle;
                    // Stretching his head out and attacking across the screen.
                    if (animations.CurrentAnimation!.currentFrameIndex == 1 && previousFrameIndex == 0)
                    {
                        // Set frame length to 4 seconds.
                        animations.CurrentAnimation.IsPaused = true;
                        // Put the head in place, this frame doesn't have one.
                        this.Head.Alive = true;
                        this.Head.WorldLocation = this.WorldLocation + new Vector2(104, -12);
                        initialHeadLocation = this.Head.WorldLocation;
                        this.Head.Velocity = new Vector2(150, 0);

                    }
                    else if (Head.Enabled)
                    {
                        // Fill in the back with necks.

                        if (Head.Velocity.X > 0 && (Head.WorldLocation.X - initialHeadLocation.X) > 128)
                        {
                            // Reverse the head
                            Head.Velocity *= -1;
                        }
                        if (Head.Velocity.X < 0 && (Head.WorldLocation.X - initialHeadLocation.X) < 0)
                        {
                            // The head came back, stop the animation.
                            animations.CurrentAnimation.IsPaused = false;
                            Head.Velocity = Vector2.Zero;
                            animations.Play("attackUp").FollowedBy("idle");
                            this.collisionRectangle = regularCollisionRectangle;
                            state = GooseState.Idle;
                        }
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

            if (Head.Enabled)
            {
                Head.Update(gameTime, elapsed);
            }

            foreach (var neck in Necks)
            {
                if (neck.Enabled)
                {
                    neck.Update(gameTime, elapsed);
                }
            }

            // Check collisions with the goose balls, the necks, and the head
            foreach (var ball in GooseBalls)
            {
                if (ball.Enabled && ball.CollisionRectangle.Intersects(_player.CollisionRectangle))
                {
                    _player.TakeHit(this);
                    ball.Kill();
                }
            }

            foreach (var neck in Necks)
            {
                if (neck.Enabled && neck.CollisionRectangle.Intersects(_player.CollisionRectangle))
                {
                    _player.TakeHit(this);
                }
            }

            if (Head.Enabled && Head.CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                _player.TakeHit(this);
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

            if (Head.Enabled)
            {
                Head.Draw(spriteBatch);
            }

            foreach (var neck in Necks)
            {
                if (neck.Enabled)
                {
                    neck.Draw(spriteBatch);
                }
            }

            base.Draw(spriteBatch);
        }
    }
}