using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// This haunted platform moves when the player controls it via a special tile.
    /// </summary>
    public class GhostPlatformControllable : GhostPlatformBase
    {

        public GhostPlatformControllable(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY, 4, 4)
        {
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
          
            base.Update(gameTime, elapsed);

        }
    }

}
