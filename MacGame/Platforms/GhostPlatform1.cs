using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// This haunted platform moves with the player when he's on it.
    /// </summary>
    public class GhostPlatform1 : Platform
    {

        public int Speed = 40;
        private Vector2 startingLocation;

        public string Name { get; set; } = "";

        public GhostPlatform1(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {

            var texture = content.Load<Texture2D>(@"Textures/Textures2");
            var left = new StaticImageDisplay(texture, Helpers.GetTileRect(4, 5));
            var middle = new StaticImageDisplay(texture, Helpers.GetTileRect(5, 5));
            middle.Offset = new Vector2(Game1.TileSize, 0);
            var right = new StaticImageDisplay(texture, Helpers.GetTileRect(6, 5));
            right.Offset = new Vector2(Game1.TileSize * 2, 0);
            this.DisplayComponent = new AggregateDisplay(new []{ left, middle, right });

            this.CollisionRectangle = new Rectangle(-4 * Game1.TileScale, -5 * Game1.TileScale, 24 * Game1.TileScale, 5 * Game1.TileScale);

            this.startingLocation = this.WorldLocation;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            var player = Game1.Player;
            // If the player is on this platform, set the velocity to move in the direction the player is facing.
            if (player.PlatformThatThisIsOn == this)
            {
                if(player.IsFacingLeft())
                {
                    this.velocity.X = -Speed;
                }
                else
                {
                    this.velocity.X = Speed;
                }

                if (player.InputManager.CurrentAction.up)
                {
                    this.velocity.Y = -Speed;
                }
                else if (player.InputManager.CurrentAction.down)
                {
                    this.velocity.Y = Speed;
                }
                else
                {
                    this.velocity.Y = 0;
                }
            }
            else
            {
                this.Velocity = Vector2.Zero;
            }

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
