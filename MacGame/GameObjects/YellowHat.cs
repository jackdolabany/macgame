using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class YellowHat : PlayerHat
    {
        public override string HatName => "Yellow Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(6, 1);
        protected override Rectangle backSource => Helpers.GetTileRect(7, 1);

        public YellowHat(Player player, ContentManager content) : base(player, content) { }
    }
}
