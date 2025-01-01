using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using MacGame.Platforms;
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
            Phase3,
            Dead
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
                else if (Health == 0)
                {
                    return AttackPhase.Dead;
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
        float stillBeforeFallingAfterDeathTimer = 0;
        float brickDelayTimer = 0f;

        // Set this to however many goose balls you want the goose to spit in the ball attack phase.
        int ballsToShoot = 0;
        
        public List<Enemy> GooseBalls = new List<Enemy>();
        public Vector2 idleHeadLocation;

        public int previousFrameIndex = 0;

        // the goose will stretch his head out across the screen and the necks will fill in behind.
        public CanadaGooseHead Head;
        public List<CanadaGooseNeck> Necks;
        Vector2 initialHeadLocation;

        Rectangle regularCollisionRectangle;
        Rectangle duckedDownCollisionRectangle;

        // When standing the goose's neck will be a collision rectangle that hurts Mac but he can't jump on.
        protected Rectangle standingNeckRectangle;

        // When standing there's a special rectangle for the goose's head that Mac can jump on.
        protected Rectangle standingHeadRectangle;

        // Where explosions will happen as the goose dies.
        private Rectangle explosionRectangle;

        private Player _player;

        private int MaxHealth = 6;

        // If so many seconds goes by and there's not current a spring, one will drop.
        float springTimer = 0f;
        const float maxSpringTimer = 5f;

        private bool isStanding;

        /// <summary>
        /// In phase 1 the goose will enable moving platforms.
        /// </summary>
        private List<MovingPlatform> MovingPlatforms = new List<MovingPlatform>();

        /// <summary>
        /// In phase 2 the goose will enable the breaking platforms.
        /// </summary>
        private List<BreakingPlatform> BreakingPlatforms = new List<BreakingPlatform>();

        /// <summary>
        /// In phase 3 the goose will enable the springboard to help you kill it. Seems like a bad idea, but who knows how Geese think?
        /// </summary>
        private SpringBoard SpringBoard;
        Vector2 springBoardInitialLocation;

        /// <summary>
        /// After death the Goose will reveal the sock.
        /// </summary>
        private Sock Sock;

        private bool isInitialized = false;

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
            honk.FrameLength = 0.4f;
            honk.Oscillate = true;
            animations.Add(honk);

            var repeatHonk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "repeatHonk");
            repeatHonk.LoopAnimation = true;
            repeatHonk.FrameLength = 0.4f;
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

            explosionRectangle = new Rectangle(this.CollisionRectangle.X, this.standingNeckRectangle.Y, this.CollisionRectangle.Width, CollisionRectangle.Bottom - standingNeckRectangle.Top);

            ResetIdleHonking();

            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));
            GooseBalls.Add(new CanadaGooseBall(content, cellX, cellY, player, camera));
            ExtraEnemiesToAddAfterConstructor.AddRange(GooseBalls);

            Head = new CanadaGooseHead(content, cellX, cellY, player, camera);
            ExtraEnemiesToAddAfterConstructor.Add(Head);

            Necks = new List<CanadaGooseNeck>();
            for (int i = 0; i < 10; i++)
            {
                Necks.Add(new CanadaGooseNeck(content, cellX, cellY, player, camera));
            }

            ExtraEnemiesToAddAfterConstructor.AddRange(Necks);

            idleHeadLocation = worldLocation + new Vector2(16, -176);

            springTimer = maxSpringTimer;

            // TODO: Sounds
            /*
             Getting hit
            dying
            honking
            ball bouncing
            ball hitting the wall
            springboard breaking
            explosions
             */
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
            SoundManager.PlaySound("GooseHonk");
            idleTimer = 0;
        }

        private void InitiateGooseBallAttack()
        {
            state = GooseState.GooseBallAttack;
            animations.Play("honk");
            SoundManager.PlaySound("GooseHonk");
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

        private void Initialize()
        {
            // Find a bunch of stuff we expect in the map.
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    Sock = item as Sock;
                }
            }

            if (Sock == null)
            {
                throw new Exception("You need a sock in the level!");
            }

            Sock.Enabled = false;

            foreach (var platform in Game1.CurrentLevel.Platforms)
            {
                if (platform is MovingPlatform)
                {
                    MovingPlatforms.Add(platform as MovingPlatform);
                }
                if (platform is BreakingPlatform)
                {
                    BreakingPlatforms.Add(platform as BreakingPlatform);
                }
            }

            SpringBoard = Game1.CurrentLevel.SpringBoards.Single();
            springBoardInitialLocation = SpringBoard.WorldLocation;
            isInitialized = true;
        }

        private void BreakSpringBoard()
        {
            if (SpringBoard.Enabled)
            {
                SpringBoard.Enabled = false;
                EffectsManager.SmallEnemyPop(SpringBoard.WorldCenter);
                springTimer = maxSpringTimer;
                SoundManager.PlaySound("Break");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            // Enable/disable the environmental objects
            foreach (var platform in MovingPlatforms)
            {
                platform.Enabled = attackPhase == AttackPhase.Phase1;
            }
            
            foreach (var platform in BreakingPlatforms)
            {
                if (attackPhase != AttackPhase.Phase2 || brickDelayTimer > 0f)
                {
                    platform.Enabled = false;
                }
            }

            if (brickDelayTimer > 0)
            {
                brickDelayTimer -= elapsed;
            }

            if (attackPhase == AttackPhase.Phase3)
            {
                // If there's no springboard, make one appear so the player has a chance to jump on this goon.
                if (!SpringBoard.Enabled && this.Alive && this.state != GooseState.Dying && this.state != GooseState.Dead)
                {
                    springTimer -= elapsed;
                    if (springTimer <= 0)
                    {
                        SpringBoard.WorldLocation = springBoardInitialLocation;
                        SpringBoard.Velocity = Vector2.Zero;
                        SpringBoard.Enabled = true;
                    }
                }
            }
            else
            {
                SpringBoard.Enabled = false;
            }

            // Sit there idle and honk a few times as an intro or after taking a hit.
            if (state == GooseState.IdleHonking)
            {
                if (animations.CurrentAnimationName == "honk" && animations.CurrentAnimation.FinishedPlaying)
                {
                    _honkCount--;
                    animations.Play("idle");
                }

                if (animations.CurrentAnimationName == "idle")
                {
                    idleTimer += elapsed;
                    if (idleTimer >= 0.4f && _honkCount > 0)
                    {
                        // it's the initial idle, honk a bit.
                        animations.Play("honk");
                        SoundManager.PlaySound("GooseHonk");
                        idleTimer = 0;
                    }
                    if (idleTimer > 0.8f && _honkCount == 0)
                    {
                        // we already honked, time to attack.
                        InitiateNeckAttack();
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
                                SoundManager.PlaySound("GooseHonk");
                                ShootBall();
                            }
                        }
                    }
                }
                if (animations.CurrentAnimationName == "idle")
                {
                    idleTimer += elapsed;
                    if (idleTimer >= 0.5f)
                    {
                        ResetIdleHonking();
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
                if (takeHitTimer >= 1.4f)
                {
                    takeHitTimer = 0f;
                    ResetIdleHonking();
                }
            }

            if (state == GooseState.Dying)
            {
                // Honk like crazy
                this.animations.PlayIfNotAlreadyPlaying("repeatHonk");

                // Be still for a bit and then fall down to your death.
                if (stillBeforeFallingAfterDeathTimer < 3f)
                {
                    stillBeforeFallingAfterDeathTimer += elapsed;
                }
                else
                {
                    this.velocity = new Vector2(0, 50);
                }

                // random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.2f)
                {
                    explosionTimer = 0f;
                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(explosionRectangle.Width);
                    var randomY = Game1.Randy.Next(explosionRectangle.Height);

                    var randomLocation = new Vector2(explosionRectangle.X + randomX, explosionRectangle.Y + randomY);
                    EffectsManager.AddExplosion(randomLocation);
                }

                if (this.CollisionRectangle.Top > (Game1.Camera.WorldRectangle.Bottom + 200))
                {
                    state = GooseState.Dead;
                    Sock.FadeIn();
                }
            }

            if (state == GooseState.Dead)
            {
                // TODO: Wait for some time and then send Mac back where he came from. 
                // Once you figure out what that is.
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            // Collisions with the SpringBoard will destroy it.
            if (!SpringBoard.IsPickedUp && SpringBoard.Enabled)
            {
                var collideWithGoose = SpringBoard.CollisionRectangle.Intersects(CollisionRectangle);
                var collideWithHead = SpringBoard.CollisionRectangle.Intersects(Head.CollisionRectangle) && Head.Enabled;

                if (collideWithGoose || collideWithHead)
                {
                    BreakSpringBoard();
                }
            }

            isStanding = this.collisionRectangle == regularCollisionRectangle;

            // Check custom collisions with the standing head and neck rectangles
            // Warning: This mimics some logic in the Player class.
            if (isStanding && Alive && Enabled && state != GooseState.TakingHit && state != GooseState.Dying && state != GooseState.Dead)
            {
                bool interactedWithHead = false;
                if (_player.CollisionRectangle.Intersects(standingHeadRectangle))
                {
                    if (_player.JumpedOnEnemyRectangle(standingHeadRectangle))
                    {
                        TakeHit(_player, 1, Vector2.Zero);
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

            var onScreen = !Game1.Camera.IsWayOffscreen(this.CollisionRectangle);

            // Honk for the repeat honk animation
            if (animations.CurrentAnimationName == "repeatHonk" && animations.CurrentAnimation.currentFrameIndex == 0 && previousFrameIndex != 0 && onScreen)
            {
                SoundManager.PlaySound("GooseHonk");
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
                    else
                    {
                        // Make sure the animation isn't paused on some other frame.
                        animations.CurrentAnimation.IsPaused = false;
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

                    if (idleTimer >= 1f)
                    {
                        InitiateGooseBallAttack();
                        idleTimer = 0f;
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

        public override void AfterHittingPlayer()
        {
            base.AfterHittingPlayer();
            
            // yeet mac
            _player.Velocity = new Vector2(500, -800);
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {

            var initialPhase = this.attackPhase;

            Health -= damage;

            SoundManager.PlaySound("GooseHit");

            // Set the brick delay timer as we transition from phase 1 to 2, so the breaking bricks don't show up right away.
            if (initialPhase == AttackPhase.Phase1 && attackPhase == AttackPhase.Phase2)
            {
                brickDelayTimer += 6f;
            }

            // Yeet the player to the right.
            _player.Velocity = new Vector2(500, -800);

            if (Health > 0)
            {
                this.state = GooseState.TakingHit;
                animations.Play("takeHit");
                takeHitTimer = 0f;

                if (!IsTempInvincibleFromBeingHit)
                {
                    InvincibleTimer += 3f;
                }
            }
            else
            {
                this.state = GooseState.Dying;
                this.Dead = true;
                explosionTimer = 0f;
                stillBeforeFallingAfterDeathTimer = 0;

            }

            // Break the spring and goose balls
            BreakSpringBoard();
            foreach (var ball in GooseBalls)
            {
                if (ball.Enabled)
                {
                    ball.Kill();
                }
            }
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