using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace MacGame.Npcs
{
    public abstract class Bully : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        protected abstract int TextureX { get; }
        protected abstract int TextureY { get; }
        protected abstract string BullyMessage { get; }
        protected abstract string BullyMessageAfterNerdHit { get; }
        protected abstract Color FruitColor { get; }

        private Vector2 _fruitLocation;
        private Vector2 _fruitVelocity;
        private bool _fruitInFlight;
        private StaticImageDisplay _fruitDisplay;
        private Nerd _nerd;
        private float _throwTimer;
        private float _nextThrowTime;
        private float _jumpTimer;
        private float _nextJumpTime;
        private static Random _random = new Random();
        private bool _isThrowing = false;
        private bool _isInitialized = false;

        public Bully(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            // Idle animation with 2 frames
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(TextureX, TextureY), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            // Throwing animation
            var throwing = new AnimationStrip(textures, Helpers.GetTileRect(TextureX + 2, TextureY), 1, "throwing");
            throwing.LoopAnimation = false;
            throwing.FrameLength = 0.3f;
            animations.Add(throwing);

            SetWorldLocationCollisionRectangle(8, 8);

            // Initialize fruit display (one tile right of throw animation)
            _fruitDisplay = new StaticImageDisplay(textures);
            _fruitDisplay.Source = Helpers.GetTileRect(TextureX + 3, TextureY);
            _fruitInFlight = false;

            // Set initial throw timer (random between 1-3 seconds)
            _nextThrowTime = GetRandomThrowTime();
            _throwTimer = 0;

            // Set initial jump timer (random between 2-3 seconds)
            _nextJumpTime = GetRandomJumpTime();
            _jumpTimer = 0;

            animations.Play("idle");
        }

        public override void InitiateConversation()
        {
            string message = Game1.StorageState.IsNerdHitByMac ? BullyMessageAfterNerdHit : BullyMessage;
            ConversationManager.AddMessage(message, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            // Initialize and find nerd
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            // Handle throwing animation finishing
            if (_isThrowing && animations.CurrentAnimationName == "idle")
            {
                _isThrowing = false;
            }

            // Update fruit physics if in flight
            if (_fruitInFlight)
            {
                // Apply gravity
                _fruitVelocity.Y += 600 * elapsed;

                // Update position
                _fruitLocation += _fruitVelocity * elapsed;

                // Check collision with nerd
                if (_nerd != null)
                {
                    Rectangle fruitRect = new Rectangle((int)_fruitLocation.X - 4, (int)_fruitLocation.Y - 4, 8, 8);
                    if (_nerd.CollisionRectangle.Intersects(fruitRect))
                    {
                        SmashFruit();
                    }
                    else
                    {
                        // Smash if it's colliding with a solid tile.
                        var cell = Game1.CurrentMap.GetMapSquareAtPixel(_fruitLocation);
                        if (cell == null || !cell.Passable)
                        {
                            SmashFruit();
                        }
                    }
                }

                // Check if fruit is off camera or hit a wall
                if (!Game1.Camera.IsObjectVisible(new Rectangle((int)_fruitLocation.X - 4, (int)_fruitLocation.Y - 4, 8, 8)))
                {
                    _fruitInFlight = false;
                }

                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(_fruitLocation);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    SmashFruit();
                }
            }

            // Update throw timer
            _throwTimer += elapsed;

            // Time to throw fruit at the nerd (only if on screen and Mac hasn't hit the nerd)
            if (_throwTimer >= _nextThrowTime && !_isThrowing && !_fruitInFlight && Game1.Camera.IsObjectVisible(this.CollisionRectangle) && !Game1.StorageState.IsNerdHitByMac)
            {
                ThrowFruit();
                _throwTimer = 0;
                _nextThrowTime = GetRandomThrowTime();
            }

            // Jump randomly in excitement while throwing fruit.
            if (!Game1.StorageState.IsNerdHitByMac)
            {
                _jumpTimer += elapsed;

                // Time to jump
                if (_jumpTimer >= _nextJumpTime && onGround)
                {
                    Jump();
                    _jumpTimer = 0;
                    _nextJumpTime = GetRandomJumpTime();
                }
            }
        }

        private void Initialize()
        {
            // Find the nerd in the level
            var npc = Game1.CurrentLevel.Npcs.FirstOrDefault(n => n is Nerd);
            if (npc != null)
            {
                _nerd = (Nerd)npc;

                // Flip to face the nerd if needed (default is facing right)
                if (_nerd.WorldCenter.X < this.WorldCenter.X)
                {
                    Flipped = true;
                }
            }
        }

        private void ThrowFruit()
        {
            _isThrowing = true;

            animations.Play("throwing").FollowedBy("idle");

            _fruitInFlight = true;
            _fruitLocation = this.WorldLocation;

            // Throw fruit toward the nerd

            float direction = _nerd.WorldCenter.X < this.WorldCenter.X ? -1 : 1;
            _fruitVelocity = new Vector2(200 * direction, -100);

            SoundManager.PlaySound("Kick");
        }

        private void SmashFruit()
        {
            _fruitInFlight = false;
            EffectsManager.EnemyPop(_fruitLocation, 5, FruitColor, 80);
            SoundManager.PlaySound("Break");
        }

        private float GetRandomThrowTime()
        {
            // Random time between 1 and 3 seconds
            return 1.0f + (float)(_random.NextDouble() * 2.0);
        }

        private float GetRandomJumpTime()
        {
            // Random time between 2 and 3 seconds
            return 2.0f + (float)(_random.NextDouble() * 1.0);
        }

        private void Jump()
        {
            // Small jump
            velocity.Y = -220;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // Draw the fruit if in flight
            if (_fruitInFlight)
            {
                float drawDepth = this.DrawDepth + Game1.MIN_DRAW_INCREMENT;
                _fruitDisplay.Draw(spriteBatch, _fruitLocation, false);
            }
        }
    }
}
