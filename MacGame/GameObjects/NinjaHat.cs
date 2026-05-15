using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class NinjaHat : PlayerHat
    {
        public override string HatName => "Ninja";

        protected override Rectangle frontSource => Helpers.GetTileRect(2, 0);
        protected override Rectangle backSource => Helpers.GetTileRect(3, 0);

        public NinjaHat(Player player, ContentManager content)
            : base(player, content)
        {
        }
    }
}
