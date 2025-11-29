using MacGame.DisplayComponents;
using MacGame.Enemies;
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
        private GameObject _gameObjectInsideChest;

        private float _resetTimer = 0f;
        private const float PICKUP_RESET_TIME = 4f;

        // When the item pops out it shouldn't be tile colliding until it clears the chest. 
        private bool wasItemTileColliding = false;
        private bool shouldItemResetTileCollisions = false;

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

            // Scan for a GameObject whose collision rectangle intersects with the chest area
            // Check Items first, then Enemies, then GameObjects
            _gameObjectInsideChest = Game1.CurrentLevel.Items.FirstOrDefault(item =>
                item.CollisionRectangle.Intersects(chestArea)
            );

            if (_gameObjectInsideChest == null)
            {
                _gameObjectInsideChest = Game1.CurrentLevel.Enemies.FirstOrDefault(enemy =>
                    enemy.CollisionRectangle.Intersects(chestArea)
                );
            }

            if (_gameObjectInsideChest == null)
            {
                _gameObjectInsideChest = Game1.CurrentLevel.GameObjects.FirstOrDefault(obj => this != obj &&
                    obj.CollisionRectangle.Intersects(chestArea)
                );
            }

            if (_gameObjectInsideChest == null)
            {
                _gameObjectInsideChest = Game1.CurrentLevel.PickupObjects.Cast<GameObject>().FirstOrDefault(obj =>
                    obj.CollisionRectangle.Intersects(chestArea)
                );
            }

            if (_gameObjectInsideChest == null)
            {
                throw new System.Exception($"No item, enemy, or object found in chest at cell ({_cellX}, {_cellY})");
            }

            // Disable the item initially and store its original location
            _gameObjectInsideChest.Enabled = false;
            SetDrawDepth(_gameObjectInsideChest.DrawDepth);

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

            // Check if we need to reset the chest
            if (isOpen)
            {
                bool shouldIncrementResetTimer = false;

                // If the item is an enemy and it died, reset the chest
                if (_gameObjectInsideChest is Enemy enemy && enemy.Dead)
                {
                    shouldIncrementResetTimer = true;
                }

                // After a few seconds you can always get a new PickUpObject and destroy the old one.
                if (_gameObjectInsideChest is IPickupObject)
                {
                    shouldIncrementResetTimer = true;
                }

                if (_gameObjectInsideChest is Item)
                {
                    if (_gameObjectInsideChest is Heart)
                    {
                        shouldIncrementResetTimer = false;
                    }
                    else if (_player.CurrentItem == null)
                    {
                        shouldIncrementResetTimer = true;
                    }
                }

                // If the item is a pickup object, handle timer and reset
                if (shouldIncrementResetTimer)
                {
                    _resetTimer += elapsed;

                    if (_resetTimer >= PICKUP_RESET_TIME)
                    {
                        CloseChest();
                    }
                }
            }

            if (!isOpen)
            {
                // Check if the pixel above the player is hitting the bottom of the chest.
                var topOfPlayer = new Rectangle(_player.CollisionRectangle.X, _player.CollisionRectangle.Top, _player.CollisionRectangle.Width, 4);
                var bottomOfChest = new Rectangle(WorldLocation.X.ToInt() - 16, WorldLocation.Y.ToInt(), 32, 4);
                if (topOfPlayer.Intersects(bottomOfChest))
                {
                    isOpen = true;
                    _resetTimer = 0f;

                    SoundManager.PlaySound("ChestOpen");

                    // Move it a bit above the chest so the player doesn't instantly collect it.
                    _gameObjectInsideChest.WorldLocation = new Vector2(this.WorldLocation.X, this.CollisionRectangle.Bottom - 8);
                    _gameObjectInsideChest.Enabled = true;

                    // Temporarily disable tile colliding until the item clears the chest.
                    wasItemTileColliding = _gameObjectInsideChest.isTileColliding;
                    _gameObjectInsideChest.isTileColliding = false;
                    shouldItemResetTileCollisions = true;

                    // Certain objects pop out of the chest.
                    if (_gameObjectInsideChest.IsAffectedByGravity && (_gameObjectInsideChest is IPickupObject || _gameObjectInsideChest is Enemy))
                    {
                        // If player is to the left, pop right. If player is to the right, pop left.
                        bool playerIsToLeft = _player.WorldLocation.X < WorldLocation.X;
                        float horizontalVelocity = playerIsToLeft ? 100f : -100f;
                        _gameObjectInsideChest.Velocity = new Vector2(horizontalVelocity, -400f);
                    }

                    if (_gameObjectInsideChest is Enemy)
                    {
                        // enemies come back to live
                        var enemy = (Enemy)_gameObjectInsideChest;
                        enemy.Alive = true;
                    }

                    _gameObjectInsideChest.ReleasedFromChest(this);
                }
            }

            // Move item up until it's just above the chest.
            if (isOpen && _gameObjectInsideChest != null && _gameObjectInsideChest is Item)
            {
                if (_gameObjectInsideChest.WorldLocation.Y > WorldLocation.Y - 28)
                {
                    _gameObjectInsideChest.Velocity = new Vector2(_gameObjectInsideChest.Velocity.X, -16);
                }
                else
                {
                    _gameObjectInsideChest.Velocity = new Vector2(_gameObjectInsideChest.Velocity.X, 0);
                }
            }

            // Reset the GameObject collisions as soon as the GameObject has cleared the tile blocking chest.
            if (shouldItemResetTileCollisions)
            {
                if (!_gameObjectInsideChest.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    _gameObjectInsideChest.isTileColliding = wasItemTileColliding;
                    shouldItemResetTileCollisions = false;
                }
            }

            base.Update(gameTime, elapsed);
        }

        private void CloseChest()
        {
            isOpen = false;
            _resetTimer = 0f;
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
                if (_gameObjectInsideChest != null && _gameObjectInsideChest.Enabled)
                {
                    _gameObjectInsideChest.Draw(spriteBatch);
                }
                OpenChestBottom.Draw(spriteBatch, WorldLocation, this.Flipped);
            }
        }
    }
}
