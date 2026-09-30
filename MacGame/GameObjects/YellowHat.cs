using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class YellowHat : PlayerHat
    {
        public override string HatName => "Yellow Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(6, 1);
        protected override Rectangle backSource => Helpers.GetBigTileRect(7, 1);

        public YellowHat(ContentManager content) : base(content) { }
    }
}
