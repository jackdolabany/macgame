using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class CanadaGooseBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private int _honkCount = 0;

        public enum GooseState
        {
            /// <summary>
            /// Goose will honk twice and be idle for a second before attacking.
            /// </summary>
            IdleHonking,
            
            /// <summary>
            /// Goose will attack you with his neck.
            /// </summary>
            NeckAttack,

            /// <summary>
            /// Goose will shoot 2 balls at you.
            /// </summary>
            GooseBallAttack,

            /// <summary>
            /// Goose is idle for a bit between attacks so you can bash him.
            /// </summary>
            IdlePauseBetweenAttacks,

            /// <summary>
            /// Goose is taking a hit.
            /// </summary>
            TakingHit,

            /// <summary>
            /// Explosions everywhere!
            /// </summary>
            Dying,
            
            /// <summary>
            /// Reveal the sock!
            /// </summary>
            Dead
        }

        /// <summary>
        /// Goose attack is 3 phases. 
        /// Phase 1 he just puts his neck down at you and then pauses with a chance for you to attack him.
        /// In Phase 2 he shoots 2 balls and gives you an idle phase before reaching out with the neck to attack you.
        /// In phase 3 he shoots 2 balls and then does the neck, without giving you a long chance to attack. You have to wait for the neck and dodge the balls. Good luck!
        /// </summary>
        public enum AttackPhase
        {
            Phase1,
            Phase2,
            Phase3
        }

        private GooseState state = GooseState.IdleHonking;
        
        private AttackPhase attackPhase
        {
            get
            {
                if (Health == 2 || Health == 1)
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

        float idleTimer = 0;
        float takeHitTimer = 0;
        float explosionTimer = 0;
        
        // Set this to however many goose balls you want the goose to spit in the ball attack phase.
        int ballsToShoot = 0;
        
        public List<Enemy> GooseBalls = new List<Enemy>();
        public Vector2 idleHeadLocation;

        public int previousFrameIndex = 0;

        // the goose will stretch his head out across the screen and the necks will fill in behind.
        public CanadaGooseHead Head;
        public List<CanadaGooseNeck> Necks;
        Vector2 initialHeadLocation;

        // The Goose will give you a springboard to help kill him. Seems like a bad idea, but who knows how Geese think?
        public SpringBoard SpringBoard;

        Rectangle regularCollisionRectangle;
        Rectangle duckedDownCollisionRectangle;

        // When standing the goose's neck will be a collision rectangle that hurts Mac but he can't jump on.
        protected Rectangle standingNeckRectangle;

        // When standing there's a special rectangle for the goose's head that Mac can jump on.
        protected Rectangle standingHeadRectangle;

        private Player _player;

        private int MaxHealth = 5;

        // If so many seconds goes by and there's not current a spring, one will drop.
        float springTimer = 0f;
        const float maxSpringTimer = 7f;

        private bool isStanding;

        public CanadaGooseBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(1, 1), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            var honk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "honk");
            honk.LoopAnimation = false;
            honk.FrameLength = 0.3f;
            honk.Oscillate = true;
            animations.Add(honk);

            var repeatHonk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "repeatHonk");
            repeatHonk.LoopAnimation = false;
            repeatHonk.FrameLength = 0.14f;
            animations.Add(repeatHonk);

            var neckAttack = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 1), 2, "neckAttack");
            neckAttack.LoopAnimation = false;
            neckAttack.FrameLength = 0.14f;
            animations.Add(neckAttack);

            var neckAttackUp = (AnimationStrip)neckAttack.Clone();
            neckAttackUp.Name = "neckAttackUp";
            neckAttackUp.Reverse = true;
            animations.Add(neckAttackUp);

            var takeHit = new AnimationStrip(textures, Helpers.GetMegaTileRect(4, 1), 1, "takeHit");
            takeHit.LoopAnimation = false;
            takeHit.FrameLength = 0.8f;
            animations.Add(takeHit);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            regularCollisionRectangle = new Rectangle(-120, -100, 110, 100);
            duckedDownCollisionRectangle = new Rectangle(-120, -100, 170, 100);
            collisionRectangle = regularCollisionRectangle;

            standingNeckRectangle = new Rectangle(this.WorldLocation.X.ToInt() - 50, this.worldLocation.Y.ToInt() - regularCollisionRectangle.Height - 85, 40, 85);
            standingHeadRectangle = new Rectangle(standingNeckRectangle.X, standingNeckRectangle.Y, 70, 20);

            ResetIdleHonking();

            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));
            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));

            Head = new CanadaGooseHead(content, cellX, cellY, player, camera);
            Necks = new List<CanadaGooseNeck>();
            for (int i = 0; i < 10; i++)
            {
                Necks.Add(new CanadaGooseNeck(content, cellX, cellY, player, camera));
            }

            idleHeadLocation = worldLocation + new Vector2(16, -176);

            SpringBoard = new SpringBoard(content, 0, 0, _player);
            SpringBoard.Enabled = false;

            springTimer = maxSpringTimer;
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        /// <summary>
        /// Sets the goose to honk a couple of times and sit idle before attacking. Use
        /// this to initiate the goose and reset after taking a hit.
        /// </summary>
        private void ResetIdleHonking()
        {
            idleTimer = 0;
            animations.Play("idle");
            _honkCount = 2;
            state = GooseState.IdleHonking;
        }

        private void InitiateNeckAttack()
        {
            state = GooseState.NeckAttack;
            animations.Play("neckAttack");
            idleTimer = 0;
        }

        private void InitiateGooseBallAttack()
        {
            state = GooseState.GooseBallAttack;
            animations.Play("honk");
            ballsToShoot = 2;
            idleTimer = 0;
            ShootBall();
        }

        private void ShootBall()
        {
            // When we honk shoot an idle ball when the frame swaps from 0 to 1.
            foreach (var ball in GooseBalls)
            {
                if (!ball.Enabled || ball.Dead)
                {
                    ball.Enabled = true;
                    ball.Dead = false;

                    var xVelocity = Helpers.GetRandomValue(new int[] { 150, 200, 120 });
                    var yVelocity = -Game1.Randy.Next(0, 400);
                    ball.Velocity = new Vector2(xVelocity, yVelocity);

                    ball.WorldLocation = idleHeadLocation + new Vector2(0, 16);

                    break;
                }
            }
        }

        private void InitiateIdleBetweenAttacks()
        {
            state = GooseState.IdlePauseBetweenAttacks;
            animations.Play("idle");
            idleTimer = 0f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            // If there's no springboard, make one appear so the player has a chance to jump on this goon.
            if (!SpringBoard.Enabled)
            {
                springTimer -= elapsed;
                if (springTimer <= 0)
                {
                    SpringBoard.WorldLocation = this.WorldLocation + new Vector2(328, -448);
                    SpringBoard.Enabled = true;
                    // So it draws in the correct location.
                    SpringBoard.Update(gameTime, elapsed);
                }
            }

            // Sit there idle and honk a few times as an intro or after taking a hit.
            if (state == GooseState.IdleHonking)
            {
                if (animations.CurrentAnimationName == "honk" && animations.CurrentAnimation.FinishedPlaying)
                {
                    _honkCount--;
                    if (_honkCount <= 0)
                    {
                        animations.Play("idle");
                    }
                    else
                    {
                        animations.Play("honk");
                    }
                }

                if (animations.CurrentAnimationName == "idle")
                {
                    idleTimer += elapsed;
                    if (idleTimer >= 1.5f)
                    {
                        if (_honkCount > 0)
                        {
                            // it's the initial idle, honk a bit.
                            animations.Play("honk");
                        }
                        else
                        {
                            // we already honked, time to attack.
                            InitiateNeckAttack();
                        }
                    }
                }
            }

            // Honk a few times and spit out bouncing goose balls.
            if (state == GooseState.GooseBallAttack)
            {
                if (animations.CurrentAnimationName == "honk")
                {
                    if (animations.CurrentAnimation.FinishedPlaying)
                    {
                        idleTimer += elapsed;

                        if (idleTimer >= 0.5f)
                        {
                            idleTimer = 0f;
                            ballsToShoot--;
                            if (ballsToShoot <= 0)
                            {
                                animations.Play("idle");
                            }
                            else
                            {
                                animations.Play("honk");
                                ShootBall();
                            }
                        }
                    }
                }
                if (animations.CurrentAnimationName == "idle")
                {
                    idleTimer += elapsed;
                    if (idleTimer >= 1.5f)
                    {
                        switch (attackPhase)
                        {
                            case AttackPhase.Phase1:
                            case AttackPhase.Phase2:
                                // In phase 2, take a small pause. You shouldn't really be here in phase 1.
                                ResetIdleHonking();
                                break;
                            case AttackPhase.Phase3:
                                // No break in phase 3.
                                InitiateNeckAttack();
                                break;
                        }
                    }
                }
            }

            // Sit idle before doing the neck attack.
            if (state == GooseState.IdlePauseBetweenAttacks)
            {
                idleTimer += elapsed;
                if (idleTimer >= 4f)
                {
                    InitiateNeckAttack();
                }
            }

            if (state == GooseState.TakingHit)
            {
                takeHitTimer += elapsed;
                if (takeHitTimer >= 0.8f)
                {
                    takeHitTimer = 0f;
                    ResetIdleHonking();
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

            Game1.DrawBossHealth = this.Alive;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            // Collisions with the SpringBoard will destroy it.
            if (!SpringBoard.IsPickedUp && SpringBoard.Enabled)
            {
                var collideWithGoose = SpringBoard.CollisionRectangle.Intersects(CollisionRectangle);
                var collideWithHead = SpringBoard.CollisionRectangle.Intersects(Head.CollisionRectangle) && Head.Enabled;

                if (collideWithGoose || collideWithHead)
                {
                    SpringBoard.Enabled = false;
                    EffectsManager.SmallEnemyPop(SpringBoard.WorldCenter);
                    springTimer = maxSpringTimer;
                }
            }

            isStanding = this.collisionRectangle == regularCollisionRectangle;

            // Check custom collisions with the standing head and neck rectangles
            // Warning: This mimics some logic in the Player class.
            if (isStanding)
            {
                bool interactedWithHead = false;
                if (_player.CollisionRectangle.Intersects(standingHeadRectangle))
                {
                    if (_player.JumpedOnEnemyRectangle(standingHeadRectangle))
                    {
                        TakeHit(1, Vector2.Zero);
                        interactedWithHead = true;
                    }
                    else
                    {
                        _player.TakeHit(this);
                        this.AfterHittingPlayer();
                        interactedWithHead = true;
                    }
                }

                if (!interactedWithHead && _player.CollisionRectangle.Intersects(standingNeckRectangle))
                {
                    _player.TakeHit(this);
                    this.AfterHittingPlayer();
                }
            }

            var previousAnimationName = animations.CurrentAnimationName;
            previousFrameIndex = animations.CurrentAnimation!.currentFrameIndex;

            base.Update(gameTime, elapsed);

            if (animations.CurrentAnimationName != previousAnimationName)
            {
                previousFrameIndex = -1;
            }

            // Atttack by stretching your neck across the screen.
            // This has to be right after the base update otherwise the enabling/disabling of the head is 
            // going to look off.
            if (state == GooseState.NeckAttack)
            {
                if (animations.CurrentAnimationName == "neckAttack")
                {
                    CollisionRectangle = duckedDownCollisionRectangle;

                    // Stretching his head out and attacking across the screen.
                    if (animations.CurrentAnimation!.currentFrameIndex == 1 && previousFrameIndex == 0)
                    {
                        // Pause the animation on the down frame while the head moves across the screen.
                        animations.CurrentAnimation.IsPaused = true;

                        // Put the head in place, this frame of the goose doesn't have one.
                        this.Head.Alive = true;
                        Head.Enabled = true;
                        this.Head.WorldLocation = this.WorldLocation + new Vector2(120, -12);
                        initialHeadLocation = this.Head.WorldLocation;
                        this.Head.Velocity = new Vector2(150, 0);
                    }
                    else if (Head.Enabled)
                    {
                        if (Head.Velocity.X > 0 && (Head.WorldLocation.X - initialHeadLocation.X) > 128)
                        {
                            // Reverse the head once it gets far enough away.
                            Head.Velocity *= -1;
                        }
                        if (Head.Velocity.X < 0 && (Head.WorldLocation.X - initialHeadLocation.X) < 0)
                        {
                            // The head came back, stop the animation.
                            animations.CurrentAnimation.IsPaused = false;
                            Head.Velocity = Vector2.Zero;
                            animations.Play("neckAttackUp").FollowedBy("idle");
                            this.collisionRectangle = regularCollisionRectangle;
                            idleTimer = 0f;
                        }
                    }
                }
                else if (animations.CurrentAnimationName == "neckAttackUp")
                {
                    // Disable the head as the goose moves up.
                    if (animations.CurrentAnimation!.currentFrameIndex == 1 && previousFrameIndex == 0)
                    {
                        Head.Enabled = false;
                    }
                }
                else if (animations.CurrentAnimationName == "idle")
                {
                    // Animation is back to idle, the attack should be done at this point.
                    idleTimer += elapsed;

                    switch (attackPhase)
                    {
                        case AttackPhase.Phase1:
                            // In phase one just hold idle for a bit before attacking again.
                            if (idleTimer >= 4f)
                            {
                                idleTimer = 0;
                                animations.Play("neckAttack");
                            }
                            break;
                        case AttackPhase.Phase2:
                        case AttackPhase.Phase3:
                            // Start the ball attack right after.
                            if (idleTimer >= 1f)
                            {
                                InitiateGooseBallAttack();
                                idleTimer = 0f;
                            }

                            break;
                    }
                }
            }
            else
            {
                Head.Enabled = false;
            }

            if (Head.Enabled)
            {
                // Fill in between the head and body with glorious necks
                var gapRemaining = Head.WorldLocation.X - initialHeadLocation.X + 8;
                var nextNeckPosition = initialHeadLocation + new Vector2(-32, -16);
                if (gapRemaining > 0)
                {
                    foreach (var neck in Necks)
                    {
                        if (gapRemaining >= 0)
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
        }

        public override void TakeHit(int damage, Vector2 force)
        {
            base.TakeHit(damage, force);
            // Yeet the player to the right.
            _player.Velocity = new Vector2(500, -800);
            this.state = GooseState.TakingHit;
            animations.Play("takeHit");
            takeHitTimer = 0f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the neck rectangle.
            if (isStanding && DrawCollisionRect || Game1.DrawAllCollisionRects)
            {
                Color color = Color.Red * 0.25f;
                spriteBatch.Draw(Game1.TileTextures, standingNeckRectangle, Game1.WhiteSourceRect, color);
            }

            // Draw the head rectangle.
            if (isStanding && DrawCollisionRect || Game1.DrawAllCollisionRects)
            {
                Color color = Color.Red * 0.25f;
                spriteBatch.Draw(Game1.TileTextures, standingHeadRectangle, Game1.WhiteSourceRect, color);
            }

            base.Draw(spriteBatch);
        }
    }
}