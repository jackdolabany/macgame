using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Represents a blocking tile on the map that can be "unlocked" by a key of the
    /// corresponding color.
    /// </summary>
    public abstract class Keyblock : GameObject
    {

        private Player _player;

        protected StaticImageDisplay lockedImage;
        protected StaticImageDisplay unlockedImage;
        int _cellX;
        int _cellY;

        bool _isLocked = true;

        private bool _isInitialized = false;

        public Keyblock(ContentManager content, int cellX, int cellY, Player player, bool isLocked) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            // Extend the collision rectangle by 4 pixels in all directions since the tile is blocking it can 
            // never interact with the player as is.
            this.CollisionRectangle = new Rectangle((-TileMap.TileSize / 2) - 4, -TileMap.TileSize - 4, TileMap.TileSize + 8, TileMap.TileSize + 8);

            _player = player;

            _isLocked = isLocked;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _isInitialized = true;
                Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = !_isLocked;
                if (_isLocked)
                {
                    DisplayComponent = lockedImage;
                }
                else
                {
                    DisplayComponent = unlockedImage;
                }
            }

            if (_isLocked)
            {
                if (HasKey() && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    _isLocked = false;
                    this.DisplayComponent = unlockedImage;
                    Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY).Passable = true;
                    
                    SoundManager.PlaySound("Unlock");

                    Game1.StorageState.AddUnblockedMapSquare(Game1.CurrentLevel.LevelNumber, Game1.CurrentLevel.Name, _cellX, _cellY);
                    StorageManager.TrySaveGame();
                }
            }
        }

        protected abstract bool HasKey();

    }
}
