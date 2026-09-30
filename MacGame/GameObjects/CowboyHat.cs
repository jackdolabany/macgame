using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class CowboyHat : PlayerHat
    {
        public override string HatName => "Cowboy Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(6, 0);
        protected override Rectangle backSource => Helpers.GetBigTileRect(7, 0);

        public CowboyHat(ContentManager content) : base(content) { }
    }
}
