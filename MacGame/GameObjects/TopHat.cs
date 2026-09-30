using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class TopHat : PlayerHat
    {
        public override string HatName => "Top Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(2, 1);
        protected override Rectangle backSource => Helpers.GetBigTileRect(3, 1);

        public TopHat(ContentManager content) : base(content) { }
    }
}
