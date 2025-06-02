using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Represents a non-blocking tile that can become blocking if a button is pressed.
    /// </summary>
    public class GhostBlock : GameObject
    {
        protected StaticImageDisplay solidImage;
        protected StaticImageDisplay passableImage;
        int _cellX;
        int _cellY;

        bool _isSolid = false;

        private bool _isInitialized = false;

        public string Name { get; set; }

        public GhostBlock(ContentManager content, int cellX, int cellY) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 8);

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            solidImage = new StaticImageDisplay(textures, Helpers.GetTileRect(14, 28));
            passableImage = new StaticImageDisplay(textures, Helpers.GetTileRect(14, 29));
            this.DisplayComponent = passableImage;

        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _isInitialized = true;
                Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = !_isSolid;
                if (_isSolid)
                {
                    DisplayComponent = solidImage;
                }
                else
                {
                    DisplayComponent = passableImage;
                }
            }
        }

        public void Open()
        {
            var cell = Game1.CurrentLevel.Map.GetMapSquareAtCell(_cellX, _cellY);
            cell.Passable = true;
            this.DisplayComponent = passableImage;
        }

        public void Solid()
        {
            var cell = Game1.CurrentLevel.Map.GetMapSquareAtCell(_cellX, _cellY);
            cell.Passable = false;
            this.DisplayComponent = solidImage;

            // Kill any enemy colliding
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    enemy.TakeHit(this, 10000, Vector2.Zero);
                }
            }

            // Destroy any pickup object colliding
            foreach (var pickupObject in Game1.CurrentLevel.PickupObjects)
            {
                if (!pickupObject.IsPickedUp && pickupObject.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    pickupObject.BreakAndReset();
                }
            }

        }

    }
}
