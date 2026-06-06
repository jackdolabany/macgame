using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class GraySmoke2 : BaseSmoke
    {
        public GraySmoke2(ContentManager content) : base(content)
        {
            NoDrawTimerGoal = 0.5f;
        }

        protected override Rectangle GetTileRect()
        {
            return Helpers.GetTileRect(3, 15);
        }
    }
}
