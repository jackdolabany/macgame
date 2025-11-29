using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using TileEngine;

namespace MacGame.Items
{
    public class Chest : GameObject
    {
        private StaticImageDisplay ClosedChest;
        private StaticImageDisplay OpenChestBottom;
        private StaticImageDisplay OpenChestTop;

        private bool isOpen = false;
        private bool isInitialized = false;

        private int _cellX;
        private int _cellY;

        private Player _player;
        private GameObject _item;

        public Chest(ContentManager content, int cellX, int cellY, Player player)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            _cellX = cellX;
            _cellY = cellY;
            _player = player;

            Enabled = true;

            // Initialize the chest images.
            ClosedChest = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(14, 1));
            OpenChestBottom = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 1));
            OpenChestTop = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 0));

            // Set to NoDisplay because we'll manually draw the static images above.
            this.DisplayComponent = new NoDisplay();
            this.SetCenteredCollisionRectangle(8, 8, 8, 8);
        }

        private void Initialize()
        {
            // Chests are blocking and only open up when the player jumps on them from below.
            var cell = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY);
            if (cell != null)
            {
                cell.Passable = false;
            }

            // Create a collision rectangle that covers the chest and one tile above
            var chestArea = this.CollisionRectangle;
            chestArea.Y -= TileMap.TileSize;
            chestArea.Height += TileMap.TileSize;

            // Scan for an item whose collision rectangle intersects with the chest area
            _item = Game1.CurrentLevel.Items.First(item =>
                item.CollisionRectangle.Intersects(chestArea)
            );

            // Disable the item initially
            _item.Enabled = false;

            SetDrawDepth(_item.DrawDepth);

            isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            ClosedChest.Update(gameTime, elapsed);
            OpenChestBottom.Update(gameTime, elapsed);
            OpenChestTop.Update(gameTime, elapsed);

            if (!isOpen)
            {
                // Check if the pixel above the player is hitting the bottom of the chest.
                var topOfPlayer = new Rectangle(_player.CollisionRectangle.X, _player.CollisionRectangle.Top, _player.CollisionRectangle.Width, 4);
                var bottomOfChest = new Rectangle(WorldLocation.X.ToInt() - 16, WorldLocation.Y.ToInt(), 32, 4);
                if (topOfPlayer.Intersects(bottomOfChest))
                {
                    isOpen = true;

                    SoundManager.PlaySound("ChestOpen");

                    if (_item != null)
                    {
                        // Move it just above the player so he doesn't instantly collect it.
                        _item.WorldLocation = new Vector2(_item.WorldLocation.X, _player.CollisionRectangle.Top - 8);
                        _item.Enabled = true;
                    }
                }
            }

            // Move item up until it's just above the chest.
            if (isOpen && _item != null && _item.WorldLocation.Y > WorldLocation.Y - 28)
            {
                _item.Velocity = new Vector2(_item.Velocity.X, -16);
            }
            else if (_item != null)
            {
                _item.Velocity = new Vector2(_item.Velocity.X, 0);
            }

            base.Update(gameTime, elapsed);
        }

        public override void SetDrawDepth(float depth)
        {
            ClosedChest.DrawDepth = depth;
            OpenChestTop.DrawDepth = depth + Game1.MIN_DRAW_INCREMENT;
            OpenChestBottom.DrawDepth = depth - Game1.MIN_DRAW_INCREMENT;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;

            if (!isOpen)
            {
                // Chest renders as closed until it's opened.
                ClosedChest.Draw(spriteBatch, WorldLocation, this.Flipped);
            }
            else
            {
                // Draw the item between the open top and bottom of the chest.
                OpenChestTop.Draw(spriteBatch, WorldLocation, this.Flipped);
                if (_item != null && _item.Enabled)
                {
                    _item.Draw(spriteBatch);
                }
                OpenChestBottom.Draw(spriteBatch, WorldLocation, this.Flipped);
            }
        }
    }
}
