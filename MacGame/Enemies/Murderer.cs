using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Kinda like Jason from Friday the 13th, but certainly not him.
    /// </summary>
    public class Murderer : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        // Tracks how long the player has been inside the murderer rectangle
        float playerInRectTimer = 0f;
        float playerInRectTimerGoal = 3f;

        // Margin in pixels to spawn off-screen
        const int SpawnMargin = 32;

        /// <summary>
        /// This is the Rectangle that Mac needs to be inside for the murderer to appear.
        /// In Tiled place a rectangle and call it "MurdererRectangle"
        /// </summary>
        Rectangle murdererRectangle;

        private bool _isInitialized = false;
        private Player _player;

        private MurdererState _state;

        private float _speed = 60f;

        // When attacking he'll jump every few seconds.
        private float _jumpTimer = 0;
        private float _jumpTimerGoal = 3;

        // Don't turn more than once per second so he doesn't spaz.
        private float _turnAroundTimer = 0f;
        private float _turnAroundTimerGoal = 1f;

        private float _throwSickleTimer = 0f;
        private float _throwSickleTimerGoal = 4f;

        /// <summary>
        /// Sickle that he randomly throws.
        /// </summary>
        private Sickle _sickle;

        private enum MurdererState
        {
            Hiding, 
            Attacking,
            Dying,
            Dead
        }

        public Murderer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            
            var walk = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 3), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            var toss = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(2, 3), 1, "toss");
            toss.LoopAnimation = false;
            toss.FrameLength = 0.14f;
            animations.Add(toss);

            // Face the player. Either to die or to turn around.
            var face = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(3, 3), 1, "face");
            face.LoopAnimation = false;
            face.FrameLength = 0.2f;
            animations.Add(face);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 10;
            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(10, 23);

            this.Enabled = false;
            _state = MurdererState.Hiding;

            _sickle = new Sickle(content, cellX, cellY, player, camera);
            _sickle.Enabled = false;
        }

        private void Initialize()
        {
            // Find the murderer rectangle in the map.
            foreach (var obj in Game1.CurrentMap.ObjectModifiers)
            {
                if (obj.Name == "MurdererRectangle")
                {
                    murdererRectangle = obj.GetScaledRectangle();
                }
            }

            if (murdererRectangle == Rectangle.Empty)
            {
                throw new Exception("The murderer needs a rectangle named 'MurdererRectangle' on the map to know where to spawn.");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            // Only handle spawn logic if not enabled and alive
            if (_state == MurdererState.Hiding)
            {
                // Check if player is inside the murderer rectangle
                if (murdererRectangle.Contains(_player.CollisionCenter))
                {
                    playerInRectTimer += elapsed;
                    if (playerInRectTimer >= playerInRectTimerGoal)
                    {
                        // Determine which side to spawn on
                        int spawnX;

                        // Y is tricky, just spawn him in the middle of the bounding rect, he's off screen so he'll just fall to the ground.
                        int spawnY = murdererRectangle.Bottom - 8;

                        // Figure out if the player is in the left, middle, or right third of the bounding rect
                        bool isLeftSpawn = false;

                        if (_player.WorldLocation.X < (murdererRectangle.X + (murdererRectangle.Width / 3f)))
                        {
                            // You're too close to the left, spawn on the right.
                            isLeftSpawn = false;
                        }
                        else if (_player.WorldLocation.X > (murdererRectangle.X + murdererRectangle.Width * (2f / 3f)))
                        {                             
                            // You're too close to the right, spawn on the left.
                            isLeftSpawn = true;
                        }
                        else
                        {
                            // You're in the middle, spawn is random.
                            isLeftSpawn = Game1.Randy.NextBool();
                        }

                        if (isLeftSpawn)
                        {
                            // Player is to the right, spawn on left
                            spawnX = Game1.Camera.ViewPort.X - SpawnMargin - (CollisionRectangle.Width / 2);
                        }
                        else
                        {
                            // Player is to the left, spawn on right
                            spawnX = Game1.Camera.ViewPort.Right + SpawnMargin + (CollisionRectangle.Width / 2);
                        }

                        // Set position
                        this.WorldLocation = new Vector2(spawnX, spawnY);
                        Enabled = true;
                        Alive = true;
                        _state = MurdererState.Attacking;
                        playerInRectTimer = 0f;
                        animations.Play("walk");
                    }
                }
                else
                {
                    playerInRectTimer = 0f;
                }
            }

            if (_state == MurdererState.Attacking)
            {
                // Walk towards the player
                _turnAroundTimer += elapsed;
                if (_turnAroundTimer >= _turnAroundTimerGoal)
                {
                    _turnAroundTimer = 0f;
                    var oldFlipped = Flipped;
                    Flipped = (Player.WorldLocation.X <= this.WorldLocation.X);

                    if (oldFlipped != Flipped)
                    {
                        // If we flipped, play the face animation.
                        animations.Play("face").FollowedBy("walk");
                    }
                }
                
                if (Flipped)
                {
                    Velocity = new Vector2(-_speed, Velocity.Y);
                }
                else
                {
                    Velocity = new Vector2(_speed, Velocity.Y);
                }

                // Jump at some interval.
                _jumpTimer += elapsed;
                if (_jumpTimer >= _jumpTimerGoal)
                {
                    Velocity = new Vector2(Velocity.X, -500);
                    _jumpTimer = 0f;
                }

                // Check if the player is still in the rectangle
                if (!murdererRectangle.Contains(_player.CollisionCenter))
                {
                    // Player left, go back to hiding
                    _state = MurdererState.Hiding;
                    Enabled = false;
                    return;
                }

                // Throw a sickle at the player.
                if (!_sickle.Enabled)
                {
                    _throwSickleTimer += elapsed;
                    if (_throwSickleTimer >= _throwSickleTimerGoal)
                    {
                        _throwSickleTimer = 0f;
                        // Spawn a sickle
                        bool isToTheRight = _player.WorldLocation.X > this.WorldLocation.X;
                        _sickle.Toss(this, isToTheRight);
                    }
                }

            }

            _sickle.Update(gameTime, elapsed);

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _sickle.Draw(spriteBatch);
        }
    }
}