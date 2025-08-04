using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A giant space ship that you can walk into and fly off to another world.
    /// </summary>
    public class SpaceShip : GameObject
    {

        private Player _player;
        private SpaceShipStairs _stairs;
        private SpaceShipDoor _door;

        private bool _isInitialized = false;

        public SpaceShip(ContentManager content, int x, int y, Player player) : base ()
        {
            _player = player;
            Enabled = true;

            IsAffectedByGravity = false;

            this.SetWorldLocationCollisionRectangle(6, 8);

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");

            this.DisplayComponent = new StaticImageDisplay(textures, Helpers.GetMegaTileRect(0, 4));

            this.WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            _stairs = new SpaceShipStairs(content, x, y, player);
            _door = new SpaceShipDoor(content, x, y - 1, player);

            TimerManager.AddNewTimer(3f, () =>
            {
                _stairs.RaiseStairs();
                _door.CloseDoor();

            });
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _door.SetDrawDepth(_player.DrawDepth - Game1.MIN_DRAW_INCREMENT);
                _stairs.SetDrawDepth(this.DrawDepth - Game1.MIN_DRAW_INCREMENT);
                _isInitialized = true;
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
