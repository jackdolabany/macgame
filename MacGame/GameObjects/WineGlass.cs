using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class WineGlass : GameObject
    {
        private bool hasBeenTossed = false;
        private bool hasLanded = false;

        public WineGlass(ContentManager content, int x, int y)
        {
            var texture = content.Load<Texture2D>(@"Textures\Textures2");
            this.DisplayComponent = new StaticImageDisplay(texture, Helpers.GetTileRect(4, 32));

            Enabled = true;
            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            // Not affected by gravity initially
            IsAffectedByGravity = false;
            isTileColliding = false;

            // Hack the position because I'm a hack.
            WorldLocation += new Vector2(4, -8);

            SetWorldLocationCollisionRectangle(8, 8);
        }

        public void TossGlass()
        {
            if (!hasBeenTossed)
            {
                hasBeenTossed = true;
                IsAffectedByGravity = true;
                isTileColliding = true;

                // Give it a velocity moving left and up slightly
                Velocity = new Vector2(-150, -200);
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            var wasOnGround = OnGround;

            base.Update(gameTime, elapsed);

            // Break when it hits the ground
            if (hasBeenTossed && !hasLanded && OnGround && !wasOnGround)
            {
                hasLanded = true;
                Break();
            }
        }

        private void Break()
        {
            // Play break effect and sound
            EffectsManager.SmallEnemyPop(WorldCenter);
            SoundManager.PlaySound("GlassBreak");

            // Disable the glass
            Enabled = false;
        }
    }
}
