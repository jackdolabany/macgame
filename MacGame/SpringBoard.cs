using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public class SpringBoard : GameObject
    {
        // How compressed is the spring between 0 and 1
        public float Compression { get; set; } = 0;

        StaticImageDisplay up;
        StaticImageDisplay middle;
        StaticImageDisplay down;

        Vector2 originalWorldLocation;

        public GameObject GameObjectOnMe { get; set; }

        public SpringBoard(ContentManager content, int x, int y, Player player)
        {

            //_player = player;

            up = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(13, 3));
            middle = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(14, 3));
            down = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 3));

            this.DisplayComponent = up;

            Enabled = true;

            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);
            originalWorldLocation = WorldLocation;

            this.SetCenteredCollisionRectangle(8, 8);
        }

        /// <summary>
        /// Get the height of the top of the spring board taking compression into account.
        /// </summary>
        public int TopHeight
        {
            get
            {
                // height is a min of 3 pixels
                var height = 3 * Game1.TileScale;

                // Add the remaining 5 pixels, minus the compression
                var extraHeight = (5f * Game1.TileScale * (1f - Compression)).ToInt();

                var topOfSpringBoard = this.WorldLocation.Y.ToInt() - height - extraHeight;
                return topOfSpringBoard;
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (GameObjectOnMe != null)
            {
                Compression += elapsed * 2f;
                Compression = Math.Min(1f, Compression);
            }
            else
            {
                Compression -= elapsed * 30f;
                Compression = Math.Max(0f, Compression);
            }

            if (GameObjectOnMe != null)
            {
                // Move the object on the spring board
                if (GameObjectOnMe.Velocity.Y >= 0)
                {
                    GameObjectOnMe.WorldLocation = new Vector2(GameObjectOnMe.WorldLocation.X, TopHeight);
                }
            }

            if (Compression <= 1f / 3f)
            {
                this.DisplayComponent = up;
            }
            else if (Compression <= 2f / 3f)
            {
                this.DisplayComponent = middle;
            }
            else
            {
                this.DisplayComponent = down;
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                if (Game1.DrawAllCollisionRects)
                {
                    var topOfSpringBoardRect = new Rectangle(this.CollisionRectangle.X, this.TopHeight, this.CollisionRectangle.Width, 3 * Game1.TileScale);
                    spriteBatch.Draw(Game1.TileTextures, topOfSpringBoardRect, Game1.WhiteSourceRect, Color.Yellow);
                }

                base.Draw(spriteBatch);
            }
        }

    }

}
