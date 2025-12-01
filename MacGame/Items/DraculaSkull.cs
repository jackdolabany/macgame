using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class DraculaSkull : DraculaPart
    {
        public DraculaSkull(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player, 4, 35)
        {
        }

        public override bool IsCollected => Game1.StorageState.HasDraculaSkull;

        protected override void MarkAsCollected()
        {
            Game1.StorageState.HasDraculaSkull = true;
        }
    }
}
