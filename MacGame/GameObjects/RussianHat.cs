using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public class RussianHat : PlayerHat
    {
        public override string HatName => "Russian Hat";
        protected override Rectangle frontSource => Helpers.GetTileRect(0, 2);
        protected override Rectangle backSource => Helpers.GetTileRect(1, 2);

        public RussianHat(Player player, ContentManager content) : base(player, content) { }
    }
}
