using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{

    
    /// <summary>
    /// Orange smoke puff from a crashed car.
    /// </summary>
    public class OrangeSmoke : BaseSmoke
    {

        public OrangeSmoke(ContentManager content) : base(content)
        {
            NoDrawTimerGoal = 0.5f;
        }

        protected override Rectangle GetTileRect()
        {
            return Helpers.GetTileRect(4, 0);
        }
    }

}
