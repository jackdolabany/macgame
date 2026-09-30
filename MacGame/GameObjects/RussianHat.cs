using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class RussianHat : PlayerHat
    {
        public override string HatName => "Russian Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(0, 2);
        protected override Rectangle backSource => Helpers.GetBigTileRect(1, 2);

        public RussianHat(ContentManager content) : base(content) { }
    }
}
