using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class GraySmoke1 : BaseSmoke
    {
        public GraySmoke1(ContentManager content) : base(content)
        {
            NoDrawTimerGoal = 0.6f;
        }

        protected override Rectangle GetTileRect()
        {
            return Helpers.GetTileRect(3, 14);
        }
    }
}
