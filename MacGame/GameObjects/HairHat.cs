using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class HairHat : PlayerHat
    {
        public override string HatName => "Hair Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(0, 1);
        protected override Rectangle backSource => Helpers.GetBigTileRect(1, 1);

        public HairHat(ContentManager content) : base(content) { }
    }
}
