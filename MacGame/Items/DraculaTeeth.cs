using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class DraculaTeeth : DraculaPart
    {
        public DraculaTeeth(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player, 7, 35)
        {
        }

        public override bool IsCollected => Game1.StorageState.HasDraculaTeeth;

        protected override void MarkAsCollected()
        {
            Game1.StorageState.HasDraculaTeeth = true;
        }

        protected override Color GetPopColor()
        {
            return Color.Gray;
        }
    }
}
