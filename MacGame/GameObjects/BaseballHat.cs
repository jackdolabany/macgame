using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class BaseballHat : PlayerHat
    {
        public override string HatName => "Baseball Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(4, 1);
        protected override Rectangle backSource => Helpers.GetTileRect(5, 1);

        public BaseballHat(Player player, ContentManager content) : base(player, content) { }
    }
}
