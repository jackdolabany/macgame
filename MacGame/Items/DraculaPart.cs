using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    /// <summary>
    /// Base class for Dracula parts (Heart, Skull, Rib, Eye).
    /// These are unique collectibles that persist across the entire game.
    /// </summary>
    public abstract class DraculaPart : Item
    {
        /// <summary>
        /// Whether or not the part was collected when the map was loaded. This is used to show a transparent part.
        /// Not accurate if you just collected it on the same map. Use IsCollected for that.
        /// </summary>
        public bool AlreadyCollected { get; set; } = false;

        private float bounceTimer = 0f;
        private const float bounceSpeed = 2f; // Speed of the bounce
        private const float bounceHeight = 4f; // Height of the bounce in pixels
        private Vector2 baseWorldLocation;

        /// <summary>
        /// True if you already collected this part.
        /// </summary>
        public abstract bool IsCollected { get; }

        public DraculaPart(ContentManager content, int cellX, int cellY, Player player, int tileX, int tileY)
            : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(tileX, tileY);
            SetWorldLocationCollisionRectangle(8, 8);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Store the base location for the floating animation
            baseWorldLocation = WorldLocation;

            if (IsCollected)
            {
                AlreadyCollected = true;
                this.DisplayComponent.TintColor = Color.White * 0.5f;
                // They're enabled but not collectible if they are already collected. They're kind of in a ghost mode.
                Enabled = true;
            }
        }

        public override void Collect(Player player)
        {
            if (!Enabled || AlreadyCollected) return;

            AlreadyCollected = true;

            // Mark this part as collected - subclass will handle setting the specific flag
            MarkAsCollected();

            // Save the game now that we've collected this part
            StorageManager.TrySaveGame();

            this.Enabled = false;
            EffectsManager.EnemyPop(WorldCenter, 10, GetPopColor(), 150);

            base.Collect(player);
        }

        /// <summary>
        /// Subclasses override this to set their specific storage flag.
        /// </summary>
        protected abstract void MarkAsCollected();

        /// <summary>
        /// Subclasses can override to customize the pop effect color.
        /// </summary>
        protected virtual Color GetPopColor()
        {
            return Color.White;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            // Apply ghostly floating animation
            bounceTimer += elapsed;
            float yOffset = (float)System.Math.Sin(bounceTimer * bounceSpeed) * bounceHeight;
            WorldLocation = new Vector2(baseWorldLocation.X, baseWorldLocation.Y + yOffset);
        }
    }
}
