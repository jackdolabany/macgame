using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class BaseballHat : PlayerHat
    {
        public override string HatName => "Baseball Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(4, 1);
        protected override Rectangle backSource => Helpers.GetBigTileRect(5, 1);

        public BaseballHat(ContentManager content) : base(content) { }
    }
}
