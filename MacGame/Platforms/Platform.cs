using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Platforms
{
    public class Platform : GameObject
    {
        public Vector2 PreviousLocation { get; set; }

        public Platform(ContentManager content, int cellX, int cellY)
            : base()
        {
            this.WorldLocation = new Vector2((cellX * TileMap.TileSize) + (TileMap.TileSize / 2), (cellY + 1) * TileMap.TileSize);
            this.PreviousLocation = this.worldLocation;
            Enabled = true;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = false;
            IsAffectedByGravity = false;
            isTileColliding = false; // platforms secretly are, but we'll check for those collisions manually.
            IsAffectedByPlatforms = false;
        }

        public Vector2 Delta
        {
            get
            {
                return worldLocation - PreviousLocation;
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            PreviousLocation = this.WorldLocation;
            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Check a custom rectangle because platforms images are 1 square tile size but the collision rectangle
            // may be smaller.
            if (Game1.Camera.IsObjectVisible(new Rectangle((int)this.WorldLocation.X - (Game1.TileSize / 2), (int)this.WorldLocation.Y - Game1.TileSize, Game1.TileSize, Game1.TileSize)))
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
