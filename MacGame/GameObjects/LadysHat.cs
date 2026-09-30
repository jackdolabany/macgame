using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class LadysHat : PlayerHat
    {
        public override string HatName => "Lady's Hat";
        protected override Rectangle frontSource => Helpers.GetBigTileRect(2, 2);
        protected override Rectangle backSource => Helpers.GetBigTileRect(3, 2);

        public LadysHat(ContentManager content) : base(content) { }
    }
}
