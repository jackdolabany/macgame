using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class NinjaHat : PlayerHat
    {
        public override string HatName => "Ninja";

        protected override Rectangle frontSource => Helpers.GetBigTileRect(2, 0);
        protected override Rectangle backSource => Helpers.GetBigTileRect(3, 0);

        public NinjaHat(ContentManager content)
            : base(content)
        {
        }
    }
}
