using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class CowboyHat : PlayerHat
    {
        public override string HatName => "Cowboy Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(6, 0);
        protected override Rectangle backSource => Helpers.GetTileRect(7, 0);

        public CowboyHat(Player player, ContentManager content) : base(player, content) { }
    }
}
