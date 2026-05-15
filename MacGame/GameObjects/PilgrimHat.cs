using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class PilgrimHat : PlayerHat
    {
        public override string HatName => "Pilgrim";

        protected override Rectangle frontSource => Helpers.GetTileRect(0, 0);
        protected override Rectangle backSource => Helpers.GetTileRect(1, 0);

        public PilgrimHat(Player player, ContentManager content)
            : base(player, content)
        {
        }
    }
}
