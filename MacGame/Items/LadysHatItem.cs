using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class LadysHatItem : HatItem
    {
        public override string HatName => "Lady's Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(2, 2);

        public LadysHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
