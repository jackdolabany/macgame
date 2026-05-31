using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class LadysHat : PlayerHat
    {
        public override string HatName => "Lady's Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(2, 2);
        protected override Rectangle backSource => Helpers.GetTileRect(3, 2);

        public LadysHat(Player player, ContentManager content) : base(player, content) { }
    }
}
