using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class HairHat : PlayerHat
    {
        public override string HatName => "Hair Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(0, 1);
        protected override Rectangle backSource => Helpers.GetTileRect(1, 1);

        public HairHat(Player player, ContentManager content) : base(player, content) { }
    }
}
