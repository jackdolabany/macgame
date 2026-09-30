using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class WinterHat : PlayerHat
    {
        public override string HatName => "Winter Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(4, 0);
        protected override Rectangle backSource => Helpers.GetBigTileRect(5, 0);

        public WinterHat(ContentManager content) : base(content) { }
    }
}
