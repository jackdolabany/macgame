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
            _door = new SpaceShipDoor(content, this.WorldLocation, player);

            // Testing
            TimerManager.AddNewTimer(3f, () =>
            {
                _stairs.RaiseStairs();
                _door.CloseDoor();
            });
        }

        public void AddStuffToLevel(Level level, ContentManager content)
        {
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

            _door.Update(gameTime, elapsed);
            _stairs.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _stairs.Draw(spriteBatch);
            _door.Draw(spriteBatch);
        }
    }
}
