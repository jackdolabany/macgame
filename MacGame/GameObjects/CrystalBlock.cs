using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Represents a blocking tile on the map that can be "unlocked" by a key of the
    /// corresponding color.
    /// </summary>
    public abstract class CrystalBlock : GameObject
    {

        private Player _player;

        protected StaticImageDisplay solidImage;
        protected StaticImageDisplay passableImage;
        int _cellX;
        int _cellY;

        bool _isSolid = true;

        private bool _isInitialized = false;

        public CrystalBlock(ContentManager content, int cellX, int cellY) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            // Extend the collision rectangle by 4 pixels in all directions since the tile is blocking it can 
            // never interact with the player as is.
            //ethis.CollisionRectangle = new Rectangle((-TileMap.TileSize / 2) - 4, -TileMap.TileSize - 4, TileMap.TileSize + 8, TileMap.TileSize + 8);
            this.SetCenteredCollisionRectangle(8, 8, 8, 8);
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

        public void Close()
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
