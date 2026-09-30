using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class PilgrimHat : PlayerHat
    {
        public override string HatName => "Pilgrim";

        protected override Rectangle frontSource => Helpers.GetBigTileRect(0, 0);
        protected override Rectangle backSource => Helpers.GetBigTileRect(1, 0);

        public PilgrimHat(ContentManager content)
            : base(content)
        {
        }
    }
}
