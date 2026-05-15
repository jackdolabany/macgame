using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class CowboyHatItem : HatItem
    {
        public override string HatName => "Cowboy Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(6, 0);

        public CowboyHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
