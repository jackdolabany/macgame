using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class WinterHat : PlayerHat
    {
        public override string HatName => "Winter Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(4, 0);
        protected override Rectangle backSource => Helpers.GetTileRect(5, 0);

        public WinterHat(Player player, ContentManager content) : base(player, content) { }
    }
}
