using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class TopHat : PlayerHat
    {
        public override string HatName => "Top Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(2, 1);
        protected override Rectangle backSource => Helpers.GetTileRect(3, 1);

        public TopHat(Player player, ContentManager content) : base(player, content) { }
    }
}
