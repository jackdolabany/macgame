using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MacGame.DisplayComponents
{
    public class NoDisplay : DisplayComponent
    {

        public string Text;

        public float GetHeight()
        {
            return 0;
        }

        public float GetWidth()
        {
            return 0;
        }

        public NoDisplay()
            : base()
        {
            RotationAndDrawOrigin = new Vector2(0, 0);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            return;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {
            return;
        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            return worldLocation;
        }
    }
}
