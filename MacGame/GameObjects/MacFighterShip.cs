using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Mac's personal fighter ship. This renders the big ship that Mac
    /// can jump into. Not the smaller ship when he's actually in a flying level.
    /// </summary>
    public class MacFighterShip : GameObject
    {
        private AnimationDisplay _frontDisplay;
        private StaticImageDisplay _backDisplay;

        private Player _player;

        private enum ShipState { Idle, Opening, Open, Closing, TakingOff, ReturningFromMission }
        private ShipState _state = ShipState.Idle;

        private float _onScreenTimer;
        private const float OpenDelay = 5f;

        private float _rocketTimer;
        private const float RocketTimerGoal = 0.1f;

        private static readonly Vector2 TakeOffDirection = Vector2.Normalize(new Vector2(1f, -1f));

        /// <summary>
        /// Track whether or not Mac was in the ship on the last frame. We use this to mess with his draw depth temporarily.
        /// </summary>
        bool _wasMacInShip = false;

        /// <summary>
        /// Cache the player's normal draw depth before we mess with it. This way we can put it back when needed.
        /// </summary>
        float _playerDrawDepth = 0f;

        /// <summary>
        /// True after returning from a mission. Prevents Mac from immediately taking off again
        /// before he's exited the ship at least once.
        /// </summary>
        bool _justReturnedFromMission = false;

        /// <summary>
        /// The map to transition to when the ship blasts off. Set a GoToMap property from the
        /// map editor.
        /// </summary>
        public string GoToMap { get; set; } = "";

        Platform shipPlatform;
        PlayerOnlyCollisionRectangle _leftWall;

        /// <summary>
        /// Mac is considered "in the ship" if his WorldLocation (feet) is within this rectangle.
        /// </summary>
        Rectangle inShipRectangle;

        public MacFighterShip(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base()
        {
            _player = player;

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");

            _frontDisplay = new AnimationDisplay();

            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 5), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            _frontDisplay.Add(idle);

            var open = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 5), 3, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.1f;
            _frontDisplay.Add(open);

            var close = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 5), 3, "close");
            close.LoopAnimation = false;
            close.FrameLength = 0.1f;
            close.Reverse = true;
            _frontDisplay.Add(close);

            _backDisplay = new StaticImageDisplay(textures, Helpers.GetMegaTileRect(3, 5));

            // Front display is the primary component for base rendering.
            DisplayComponent = _frontDisplay;

            SetWorldLocationCollisionRectangle(32, 24);
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);

            IsAffectedByGravity = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            Enabled = true;

            _frontDisplay.Play("idle");
        }

        /// <summary>
        /// Adds the invisible standing platform inside the cockpit. Call this after the ship's WorldLocation is set.
        /// </summary>
        public void AddStuffToLevel(Level level, ContentManager content)
        {
            shipPlatform = new Platform(content, 0, 0);
            shipPlatform.WorldLocation = WorldLocation + new Vector2(-12, -16);
            shipPlatform.CollisionRectangle = new Rectangle(0, -12, 60, 12);
            shipPlatform.DisplayComponent = new NoDisplay();
            level.Platforms.Add(shipPlatform);

            // Rectangle that controls the draw depth adjustments when mac is "in" the ship.
            inShipRectangle = new Rectangle(
               shipPlatform.CollisionRectangle.Left,
               shipPlatform.CollisionRectangle.Top - 24,
               shipPlatform.CollisionRectangle.Width + 12,
               24);

            // Invisible wall flush with the left edge of the platform. Enabled only while Mac
            // is standing on the platform so he can't walk left off the edge, but disabled the
            // moment he jumps so he can jump away freely.
            _leftWall = new PlayerOnlyCollisionRectangle();
            _leftWall.WorldLocation = new Vector2(shipPlatform.CollisionRectangle.Left - 12, shipPlatform.CollisionRectangle.Top - 32);
            _leftWall.CollisionRectangle = new Rectangle(0, 0, 12, 32);
            _leftWall.Enabled = false;
            level.CustomCollisionObjects.Add(_leftWall);
        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            // Draw the back in the back, with some padding to stuff Mac in there.
            _backDisplay.DrawDepth = depth + (10 * Game1.MIN_DRAW_INCREMENT);
        }

        private void MacExitedTheShip()
        {
            _player.SetDrawDepth(_playerDrawDepth);
            _justReturnedFromMission = false;
        }

        /// <summary>
        /// The ship's front cockpit panel renders in front of Mac; the back is behind Mac.
        /// Used when Mac is outside the cockpit rect while the ship is open.
        /// </summary>
        private void MacEnteredTheShip()
        {
            _playerDrawDepth = _player.DrawDepth;

            // Some padding for Mac's little hat or whatever.
            _player.SetDrawDepth(this.DrawDepth + (5 * Game1.MIN_DRAW_INCREMENT));
        }

        public bool IsMacInShip()
        {
            if (_state == ShipState.Idle) return false;

            if (_state == ShipState.TakingOff) return true;

            if (_state == ShipState.ReturningFromMission) return true;

            var isInShipRectangle = inShipRectangle.Contains(this._player.WorldLocation + new Vector2(0, -2));

            // Make sure Mac is also to the right of the collision wall or weird visual things will happen.
            var isToRightOfCollisionWall = this.shipPlatform.Enabled = _player.CollisionRectangle.Left >= _leftWall.CollisionRectangle.Right;

            return isInShipRectangle && isToRightOfCollisionWall;
        }

        /// <summary>
        /// Called when the player returns from a shooter level. Places Mac inside the cockpit,
        /// fixes his draw depth, disables his controls, and opens the door.
        /// </summary>
        public void ReturnMacToShip(Player player)
        {
            // Place Mac's feet at the platform surface inside the cockpit.
            player.WorldLocation = WorldLocation + new Vector2(12, -28);
            player.Velocity = Vector2.Zero;
            player.IsInvisibleAndCantMove = true;

            // Apply in-ship draw depth immediately so the first render is correct.
            MacEnteredTheShip();
            _wasMacInShip = true;
            _justReturnedFromMission = true;

            _state = ShipState.ReturningFromMission;
            TimerManager.AddNewTimer(1f, () => 
            { 
                _frontDisplay.Play("open");
                _state = ShipState.Open;
                _player.IsInvisibleAndCantMove = false;
            });
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            shipPlatform.Enabled = _state != ShipState.Idle;
            var macIsInTheShip = IsMacInShip();

            if (!_wasMacInShip && macIsInTheShip)
            {
                MacEnteredTheShip();
            }
            else if (_wasMacInShip && !macIsInTheShip)
            {
                MacExitedTheShip();
            } 

            // Wall is only active while Mac is grounded on the ship platform.
            _leftWall.Enabled = _player.PlatformThatThisIsOn == shipPlatform;

            bool isVisible = Game1.Camera.IsObjectVisible(CollisionRectangle);

            switch (_state)
            {
                case ShipState.Idle:
                    if (isVisible)
                    {
                        _onScreenTimer += elapsed;
                        if (_onScreenTimer >= OpenDelay)
                        {
                            _state = ShipState.Opening;
                            _frontDisplay.Play("open");
                        }
                    }
                    break;

                case ShipState.Opening:
                    if (_frontDisplay.CurrentAnimation?.FinishedPlaying == true)
                    {
                        _state = ShipState.Open;
                    }
                    break;


                case ShipState.Open:
                    // Blast off if the Player presses up.
                    if (!_justReturnedFromMission
                        && _player.PlatformThatThisIsOn == shipPlatform
                        && _player.InputManager.CurrentAction.up
                        && !_player.InputManager.PreviousAction.up)
                    {
                        _state = ShipState.Closing;
                        _frontDisplay.Play("close");
                        GlobalEvents.FireBeginDoorEnter(this, EventArgs.Empty);
                    }
                    break;

                case ShipState.Closing:
                    if (_frontDisplay.CurrentAnimation?.FinishedPlaying == true)
                    {
                        _state = ShipState.TakingOff;
                        _player.IsInvisibleAndCantMove = true;
                    }
                    break;

                case ShipState.TakingOff:
                    Velocity += TakeOffDirection * 1000f * elapsed;

                    _rocketTimer += elapsed;
                    if (_rocketTimer >= RocketTimerGoal)
                    {
                        _rocketTimer -= RocketTimerGoal;
                        // Exhaust trails from the bottom-left of the ship (opposite the 45 degree direction).
                        EffectsManager.AddExplosion(new Vector2(this.CollisionRectangle.Left, this.CollisionRectangle.Center.Y));
                    }

                    bool offScreen = worldLocation.Y < Game1.Camera.ViewPort.Top || worldLocation.X > Game1.Camera.ViewPort.Right;
                    if (offScreen)
                    {
                        GlobalEvents.FireDoorEntered(this, GoToMap, "", Name, Game1.TransitionType.SlowFade);
                        _state = ShipState.Idle;
                    }
                    break;
            }

            base.Update(gameTime, elapsed);

            _wasMacInShip = macIsInTheShip;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game1.DrawAllCollisionRects)
            {
                spriteBatch.Draw(Game1.TileTextures, inShipRectangle, Game1.WhiteSourceRect, Color.Orange * 0.25f);
                spriteBatch.Draw(Game1.TileTextures, _leftWall.CollisionRectangle, Game1.WhiteSourceRect, Color.Purple * 0.35f);
            }

            _backDisplay.Draw(spriteBatch, WorldLocation, Flipped);

            base.Draw(spriteBatch);
        }
    }
}
