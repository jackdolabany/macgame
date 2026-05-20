using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigShipHomingLauncher : Enemy
    {
        private const float MinIdleTime = 2f;
        private const float MaxIdleTime = 4f;
        private const float StraightTime = 0.6f;

        private float _idleTimer;
        private bool _isUpsideDown = false;

        private enum State
        {
            Idle,
            Opening,
            Closing
        }

        private State _currentState = State.Idle;

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        Vector2 missileLaunchOffset;

        /// <summary>
        /// Whether or not it will shoot.
        /// </summary>
        public bool Active { get; set; } = true;

        public BigShipHomingLauncher(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");

            DisplayComponent = new AnimationDisplay();

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(8, 7), 1, "idle");
            idle.LoopAnimation = false;
            animations.Add(idle);

            var open = new AnimationStrip(textures, Helpers.GetTileRect(8, 7), 3, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.12f;
            animations.Add(open);

            var close = (AnimationStrip)open.Clone();
            close.Name = "close";
            close.Reverse = true;
            animations.Add(close);

            animations.Play("idle");

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 4;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetWorldLocationCollisionRectangle(6, 4);

            ResetIdleTimer();

            missileLaunchOffset = new Vector2(0, 0);
        }

        /// <summary>
        /// Flips it upside down. You can't flip it again.
        /// </summary>
        public void FlipUpsideDown()
        {
            _isUpsideDown = true;
            collisionRectangle.Y -= 16;
            missileLaunchOffset.Y -= 0;
        }

        private void ResetIdleTimer()
        {
            _idleTimer = MinIdleTime + Game1.Randy.NextFloat() * (MaxIdleTime - MinIdleTime);
        }

        private bool IsPlayerOnCorrectSide()
        {
            if (!_isUpsideDown)
            {
                return Player.CollisionCenter.Y < CollisionCenter.Y;
            }
            else
            {
                return Player.CollisionCenter.Y > CollisionCenter.Y;
            }
        }

        private void LaunchMissile()
        {
            var initialDirection = _isUpsideDown ? Vector2.UnitY : -Vector2.UnitY;
            MissileManager.LaunchMissile(this.WorldLocation + missileLaunchOffset, initialDirection, StraightTime);
            PlaySoundIfOnScreen("Fire", 0.7f);
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            Enabled = false;
            PlayDeathSound();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                animations.ExtraEffects = _isUpsideDown ? SpriteEffects.FlipVertically : SpriteEffects.None;

                switch (_currentState)
                {
                    case State.Idle:
                        if (IsOnScreen() && IsPlayerOnCorrectSide() && Active)
                        {
                            _idleTimer -= elapsed;
                            if (_idleTimer <= 0f)
                            {
                                _currentState = State.Opening;
                                animations.Play("open");
                            }
                        }
                        break;

                    case State.Opening:
                        if (animations.CurrentAnimation!.FinishedPlaying)
                        {
                            LaunchMissile();
                            _currentState = State.Closing;
                            animations.Play("close");
                        }
                        break;

                    case State.Closing:
                        if (animations.CurrentAnimation!.FinishedPlaying)
                        {
                            ResetIdleTimer();
                            _currentState = State.Idle;
                            animations.Play("idle");
                        }
                        break;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
