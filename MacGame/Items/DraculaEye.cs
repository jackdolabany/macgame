using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class DraculaEye : DraculaPart
    {
        public DraculaEye(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player, 6, 35)
        {
        }

        public override bool IsCollected => Game1.StorageState.HasDraculaEye;

        protected override void MarkAsCollected()
        {
            Game1.StorageState.HasDraculaEye = true;
        }
    }
}
