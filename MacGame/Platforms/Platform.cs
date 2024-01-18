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
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize, cellY * TileMap.TileSize);
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
            if (Game1.Camera.IsObjectVisible(this.CollisionRectangle))
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
