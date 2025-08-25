using MacGame.DisplayComponents;
using MacGame.Npcs;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A giant space ship that you can walk into and fly off to another world. Not the small one you control,
    /// that's a different player sprite.
    /// </summary>
    public class SpaceShip : GameObject
    {

        private Player _player;
        private SpaceShipStairs _stairs;
        private SpaceShipDoor _door;

        private bool _isInitialized = false;

        /// <summary>
        /// Set with map modifiers in Scenemanager.cs
        /// </summary>
        public string Name { get; set; }
        public string GoToDoor { get; set; }
        public string GoToMap { get; set; }

        private SpaceShipState _state;

        float rocketTimer = 0;
        const float rocketTimerGoal = 0.1f;

        public enum SpaceShipState
        {
            Idle,
            DoorClosing,
            TakingOff,
            DoorOpening
        }

        public SpaceShip(ContentManager content, int x, int y, Player player) : base ()
        {
            _player = player;
            Enabled = true;

            IsAffectedByGravity = false;

            this.SetWorldLocationCollisionRectangle(6, 8);

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");

            this.DisplayComponent = new StaticImageDisplay(textures, Helpers.GetMegaTileRect(0, 4));

            this.WorldLocation = new Vector2((x * TileMap.TileSize) + (TileMap.TileSize / 2) + 4, (y + 1) * TileMap.TileSize);

            _stairs = new SpaceShipStairs(content, this.WorldLocation, player);
            _door = new SpaceShipDoor(content, this.WorldLocation, player, this);

            _state = SpaceShipState.Idle;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
        }

        public void AddStuffToLevel(Level level, ContentManager content)
        {
            PositionChildren();

            _door.Name = this.Name;
            _door.GoToMap = this.GoToMap;
            _door.GoToDoorName = this.GoToDoor;
            level.Doors.Add(_door);

            level.Map.GetMapSquareAtPixel(_stairs.WorldCenter).IsLadder = true;

            // Add a hidden platform at the top of ladders so you can climb to the top and stand on them.
            var ladderPlatform = new LadderPlatform(content, 0, 0);
            ladderPlatform.WorldLocation = this.WorldLocation;
            level.Platforms.Add(ladderPlatform);
        }

        public void TakeOff()
        {
            _state = SpaceShipState.DoorClosing;
            _stairs.RaiseStairs();
            _door.CloseDoor();

            // Put the player inside the ship
            _player.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);
        }

        private void PositionChildren()
        {
            _door.WorldLocation = this.WorldLocation + new Vector2(-4, -Game1.TileSize);
            _stairs.WorldLocation = this.WorldLocation + new Vector2(-4, 0);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _door.SetDrawDepth(_player.DrawDepth - Game1.MIN_DRAW_INCREMENT);

                //Game1.CurrentLevel.Doors.Add()

                _stairs.SetDrawDepth(this.DrawDepth - Game1.MIN_DRAW_INCREMENT);
                _isInitialized = true;

                _door.Name = this.Name;
                _door.GoToMap = this.GoToMap;
                _door.GoToDoorName = this.GoToDoor;
            }

            base.Update(gameTime, elapsed);

            // Adjust the position to the space ship
            PositionChildren();

            var leftRocketLocation = this.WorldLocation - new Vector2(-100, 0);
            var rightRocketLocation = this.WorldLocation - new Vector2(100, 0);


            switch (_state)
            {
                case SpaceShipState.Idle:
                    break;
                case SpaceShipState.DoorClosing:
                    if (_door.IsClosed() && _stairs.AreStairsRaised())
                    {
                        _state = SpaceShipState.TakingOff;

                        // Pretend Mac is inside the ship.
                        _player.IsInvisible = true;
                    }
                    break;
                case SpaceShipState.TakingOff:
                    // Start flying up
                    this.Velocity += new Vector2(0, -1000) * elapsed;

                    rocketTimer += elapsed;
                    if (rocketTimer >= rocketTimerGoal)
                    {
                        rocketTimer -= rocketTimerGoal;
                        EffectsManager.AddExplosion(leftRocketLocation);
                        EffectsManager.AddExplosion(rightRocketLocation);
                    }

                    if (worldLocation.Y < Game1.Camera.ViewPort.Top)
                    {
                        GlobalEvents.FireDoorEntered(this, GoToMap, GoToDoor, Name);
                        _state = SpaceShipState.Idle;
                    }

                    break;
                case SpaceShipState.DoorOpening:
                    break;
            }

            _door.Update(gameTime, elapsed);
            _stairs.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _stairs.Draw(spriteBatch);
            _door.Draw(spriteBatch);

            // Draw a blue square behind the doorway
            spriteBatch.Draw(Game1.TileTextures, 
                new Rectangle((int)this.WorldLocation.X - 20, (int)this.WorldLocation.Y - 68, 40, 40), 
                Game1.WhiteSourceRect, Pallette.DarkBlue, 0f, Vector2.Zero, SpriteEffects.None, this.DrawDepth + (10 * Game1.MIN_DRAW_INCREMENT));
        }
    }
}
