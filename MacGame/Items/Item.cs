using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    public abstract class Item : GameObject
    {
        /// <summary>
        /// This is always expected to be a static image.
        /// </summary>
        public StaticImageDisplay ItemIcon
        {
            get
            {
                return (StaticImageDisplay)this.DisplayComponent;
            }
        }

        private Camera _camera;
        protected Player _player;

        StaticImageDisplay ClosedChest;
        StaticImageDisplay OpenChestBottom;
        StaticImageDisplay OpenChestTop;

        private bool isOpen = false;
        
        // The item may move up, but the chest never moves.
        Vector2 ChestPosition;

        bool isInitialized = false;
        int _cellX;
        int _cellY;

        /// <summary>
        /// Determines whether or not the item goes back in the box if you walk the box off screen.
        /// </summary>
        protected bool IsReenabledOnceOffScreen = false;

        // Whether nor not the item is free floating or starts locked in a chest.
        protected bool IsInChest = true;

        public Item(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            ChestPosition = WorldLocation;
            _cellX = cellX;
            _cellY = cellY;

            Enabled = true;
            _player = player;
            _camera = camera;
            
            // Initialize the chest images.
            ClosedChest = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(14, 1));
            OpenChestBottom = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 1));
            OpenChestTop = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 0));
        }

        private void Collect(Player player)
        {
            WhenCollected(player);
        }

        public abstract void WhenCollected(Player player);

        /// <summary>
        /// Store the original tint for when we make it flash.
        /// </summary>
        private Color originalTint;
        private float flashTimer = 0;
        private const float flashDuration = 0.3f;
        private bool isFlashingInvisible = false;

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized && IsInChest)
            {
                // Items render in a chest, they're blocking and only open up when the player jumps on them from below.
                var cell = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY);
                if (cell != null)
                {
                    cell.Passable = false;
                }
                isInitialized = true;
            }

            if (IsInChest)
            {
                ClosedChest.Update(gameTime, elapsed);
                OpenChestBottom.Update(gameTime, elapsed);
                OpenChestTop.Update(gameTime, elapsed);
            }

            if (IsInChest && !isOpen)
            {
                // Check if the pixel above the player is hitting the bottom of the chest.
                var topOfPlayer = new Rectangle(_player.CollisionRectangle.X, _player.CollisionRectangle.Top, _player.CollisionRectangle.Width, 4);
                var bottomOfChest = new Rectangle(ChestPosition.X.ToInt() - 16, ChestPosition.Y.ToInt(), 32, 4);
                if (topOfPlayer.Intersects(bottomOfChest))
                {
                    isOpen = true;

                    // Move it just above the player so he doesn't instantly collect it.
                    this.worldLocation.Y = _player.CollisionRectangle.Top - 8;
                }
            }
            else if (Enabled)
            {
                // Check for player/item collision.
                if (_player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    this.Collect(_player);
                }
            }
            else if (IsReenabledOnceOffScreen)
            {

                // re-enable if the player doesn't have it and they item is off screen.
                if (_player.CurrentItem != this && !_camera.IsObjectVisible(this.CollisionRectangle))
                {
                    Enabled = true;
                    isOpen = false;
                    WorldLocation = ChestPosition;
                }
            }

            // Move up until it's just above the chest.
            if (IsInChest)
            {
                if (isOpen && this.worldLocation.Y > ChestPosition.Y - 28)
                {
                    this.velocity.Y = -16;
                }
                else
                {
                    this.velocity.Y = 0;
                }
            }
            base.Update(gameTime, elapsed);
        }

        public override void SetDrawDepth(float depth)
        {
            this.DisplayComponent.DrawDepth = depth;
            OpenChestTop.DrawDepth = depth + Game1.MIN_DRAW_INCREMENT;
            OpenChestBottom.DrawDepth = depth - Game1.MIN_DRAW_INCREMENT;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;

            if (IsInChest)
            { 
                if (!isOpen)
                {
                    // Item renders as a chest until it's opened.
                    ClosedChest.Draw(spriteBatch, ChestPosition, this.Flipped);
                }
                else
                {
                    // Draw the item between the open top and bottom of the chest.
                    OpenChestTop.Draw(spriteBatch, ChestPosition, this.Flipped);
                    base.Draw(spriteBatch);
                    OpenChestBottom.Draw(spriteBatch, ChestPosition, this.Flipped);
                }
            }
            else
            { 
                base.Draw(spriteBatch); 
            }
        }
    }
}
