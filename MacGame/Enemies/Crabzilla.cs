using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MacGame.Enemies
{
    public enum CrabzillaState
    {
        Unseen,
        Idle,
        ClosingArms,
        ArmsClosedWaiting,
        OpeningArms,
        Dying,
        Dead
    }

    public class Crabzilla : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        CrabzillaState _state = CrabzillaState.Unseen;

        private const int MaxHealth = 100;

        private float _idleTimer = 0f;
        private const float IdleDuration = 4f;

        private float _armsClosedWaitTimer = 0f;
        private const float ArmsClosedWaitDuration = 1f;

        private float _dyingTimer = 0f;
        private const float DyingDuration = 4f;
        private float _explosionTimer = 0f;

        private bool _isInitialized = false;
        private Sock _sock;

        int width = 222 / 3 * Game1.TileScale;
        int height = 128 * Game1.TileScale;

        /// <summary>
        ///  To know when the thing is on screen. This will be much larger than the hit box regular collision rectangle.
        /// </summary>
        Rectangle isSeenRectangle;

        public List<Rectangle> frame1CollisionRectangles;
        public List<Rectangle> frame2CollisionRectangles;
        public List<Rectangle> frame3CollisionRectangles;
        public List<Rectangle> extraCollisionRectangles;

        AnimationDisplay animationDisplay => (AnimationDisplay)DisplayComponent;

        public Crabzilla(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = true;
            CanBeJumpedOn = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Crabzilla");

            var firstFrameRect = new Rectangle(0, 0, width, height);
            var idle = new AnimationStrip(textures, firstFrameRect, 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            var closeArms = new AnimationStrip(textures, firstFrameRect, 3, "closeArms");
            closeArms.LoopAnimation = false;
            closeArms.FrameLength = 0.15f;
            
            animations.Add(closeArms);

            var openArms = (AnimationStrip)closeArms.Clone();
            openArms.Reverse = true;
            openArms.Name = "openArms";
            animations.Add(openArms);

            animations.Play("idle");

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;


            this.WorldLocation += new Vector2(0, (height / 2) - Game1.TileSize);

            // Just around the crab body
            CollisionRectangle = new Rectangle(8, -80 * Game1.TileScale, 27 * Game1.TileScale, 33 * Game1.TileScale);

            isSeenRectangle = new Rectangle(WorldLocation.X.ToInt() - (width / 2), WorldLocation.Y.ToInt() - height, width, height);

            // Figure out different collision rectangles for each frame.
            frame1CollisionRectangles = new List<Rectangle>
            {
                // Top claw
                _getRelativeCrabRectangle(-20, -500, 100, 100),
                // Top arm
                _getRelativeCrabRectangle(30, -400, 30, 76),
                // bottom claw
                _getRelativeCrabRectangle(-20, -110, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(30, -180, 30, 76),
            };

            frame2CollisionRectangles = new List<Rectangle>
            {
                // Top claw.
                _getRelativeCrabRectangle(-100, -430, 100, 100),
                // Top arm.
                _getRelativeCrabRectangle(10, -400, 30, 76),
                // bottom claw
                _getRelativeCrabRectangle(-100, -180, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(10, -180, 30, 76),
            };

            frame3CollisionRectangles = new List<Rectangle>
            {
                 // Top claw.
                _getRelativeCrabRectangle(-140, -368, 100, 100),
                // Top arm.
                _getRelativeCrabRectangle(-40, -350, 40, 30),
                // bottom claw
                _getRelativeCrabRectangle(-140, -240, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(-40, -190, 40, 30),
            };
        }

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock sock && sock.Name == "CrabzillaSock")
                {
                    _sock = sock;
                    break;
                }
            }

            if (_sock == null)
            {
                throw new Exception("You need a sock named CrabzillaSock in the level!");
            }

            if (!_sock.IsCollected)
            {
                _sock.Enabled = false;
            }

            _isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_state == CrabzillaState.Unseen)
            {
                if (Game1.Camera.IsObjectVisible(isSeenRectangle))
                {
                    _state = CrabzillaState.Idle;
                }
                else
                {
                    return;
                }
            }

            if (_state != CrabzillaState.Dead)
            {
                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = MaxHealth;
                Game1.BossHealth = Health;
                Game1.BossName = "Crabzilla";
            }

            if (_state == CrabzillaState.Idle)
            {
                _idleTimer += elapsed;
                if (_idleTimer >= IdleDuration)
                {
                    _idleTimer = 0f;
                    _state = CrabzillaState.ClosingArms;
                    animations.Play("closeArms");
                }
            }
            else if (_state == CrabzillaState.ClosingArms)
            {
                if (animations.CurrentAnimation!.FinishedPlaying)
                {
                    _armsClosedWaitTimer = 0f;
                    _state = CrabzillaState.ArmsClosedWaiting;
                }
            }
            else if (_state == CrabzillaState.ArmsClosedWaiting)
            {
                _armsClosedWaitTimer += elapsed;
                if (_armsClosedWaitTimer >= ArmsClosedWaitDuration)
                {
                    _state = CrabzillaState.OpeningArms;
                    animations.Play("openArms");
                }
            }
            else if (_state == CrabzillaState.OpeningArms)
            {
                if (animations.CurrentAnimation!.FinishedPlaying)
                {
                    _state = CrabzillaState.Idle;
                }
            }
            else if (_state == CrabzillaState.Dying)
            {
                _explosionTimer += elapsed;
                if (_explosionTimer >= 0.13f)
                {
                    _explosionTimer = 0f;
                    EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), true);
                }

                _dyingTimer += elapsed;

                if (_dyingTimer >= DyingDuration)
                {
                    Dead = true;
                    Enabled = false;
                    _state = CrabzillaState.Dead;
                    TimerManager.AddNewTimer(1f, () => { _sock.FadeIn(); });
                }
            }

            base.Update(gameTime, elapsed);

            // Set a different set of extra collision rectangles for each frame.
            if (animationDisplay.CurrentAnimationName == "idle"
                || animationDisplay.CurrentAnimationName == "openArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 2
                || animationDisplay.CurrentAnimationName == "closeArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 0)
            {
                extraCollisionRectangles = frame1CollisionRectangles;
            }
            else if (animationDisplay.CurrentAnimation.currentFrameIndex == 1)
            {
                extraCollisionRectangles = frame2CollisionRectangles;
            }
            else if (animationDisplay.CurrentAnimationName == "openArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 0
                || animationDisplay.CurrentAnimationName == "closeArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 2)
            {
                extraCollisionRectangles = frame3CollisionRectangles;
            }

            CheckExtraCollisionRectangles(extraCollisionRectangles);

            if (_state == CrabzillaState.Dying)
            {
                var deadPercentage = _dyingTimer / DyingDuration;
                DisplayComponent.TintColor = Color.Lerp(Color.White, Color.Transparent, deadPercentage);
            }
        }

        private Rectangle _getRelativeCrabRectangle(int x, int y, int width, int height)
        {
            return new Rectangle(this.WorldLocation.X.ToInt() + x,
                this.WorldLocation.Y.ToInt() + y,
                width,
                height);
        }

        public override void PlayTakeHitSound()
        {
            SoundManager.PlaySound("HitEnemy2");
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            if (!CanTakeHit()) return;
            if (_state == CrabzillaState.Dying || _state == CrabzillaState.Dead) return;

            base.TakeHit(attacker, damage);
        }

        public override void Kill()
        {
            _state = CrabzillaState.Dying;
            Attack = 0;
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawExtraDebugRectangle(spriteBatch, isSeenRectangle, Color.Orange * 0.15f);
            DrawExtraDebugRectangles(spriteBatch, extraCollisionRectangles, Color.Green * 0.5f);

            base.Draw(spriteBatch);
        }
    }
}
