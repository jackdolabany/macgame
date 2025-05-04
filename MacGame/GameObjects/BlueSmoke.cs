using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{

    
    /// <summary>
    /// Blue smoke puff from a crashed car.
    /// </summary>
    public class BlueSmoke : BaseSmoke
    {
        public BlueSmoke(ContentManager content) : base(content)
        {
            NoDrawTimerGoal = 0.6f;
        }

        protected override Rectangle GetTileRect()
        {
            return Helpers.GetTileRect(4, 1);
        }
    }

}
