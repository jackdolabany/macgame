using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// A base class for haunted movable ghost platforms.
    /// </summary>
    public class GhostPlatformBase : Platform
    {

        public int Speed = 40;
        private Vector2 startingLocation;

        public string Name { get; set; } = "";
        public string GroupName { get; set; } = "";

        public GhostPlatformBase(ContentManager content, int cellX, int cellY, int firstTileRectX, int firstTileRectY)
            : base(content, cellX, cellY)
        {

            var texture = content.Load<Texture2D>(@"Textures/Textures2");
            var left = new StaticImageDisplay(texture, Helpers.GetTileRect(firstTileRectX, firstTileRectY));
            var middle = new StaticImageDisplay(texture, Helpers.GetTileRect(firstTileRectX + 1, firstTileRectY));
            middle.Offset = new Vector2(Game1.TileSize, 0);
            var right = new StaticImageDisplay(texture, Helpers.GetTileRect(firstTileRectX + 2, firstTileRectY));
            right.Offset = new Vector2(Game1.TileSize * 2, 0);
            this.DisplayComponent = new AggregateDisplay(new []{ left, middle, right });

            this.CollisionRectangle = new Rectangle(-4 * Game1.TileScale, -5 * Game1.TileScale, 24 * Game1.TileScale, 5 * Game1.TileScale);

            this.startingLocation = this.WorldLocation;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            // Fields will destroy the platform.
            if (this.velocity != Vector2.Zero)
            {
                foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (gameObject is DestroyPickupObjectField field && field.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        this.Reset();
                        break;
                    }
                }
            }
        }

        public void MoveLeft()
        {
            this.velocity.X = -Speed;
        }

        public void MoveRight()
        {
            this.velocity.X = Speed;
        }

        public void MoveUp()
        {
            this.velocity.Y = -Speed;
        }

        public void MoveDown()
        {
            this.velocity.Y = Speed;
        }

        public void StopMoving()
        {
            this.velocity = Vector2.Zero;
        }

        public void Reset()
        {
            var needsReset = this.WorldLocation != this.startingLocation || !this.Enabled;

            this.WorldLocation = this.startingLocation;
            this.velocity = Vector2.Zero;
            this.Enabled = true;

            if (needsReset)
            {
                SoundManager.PlaySound("GhostSound");
            }
        }
    }

}
