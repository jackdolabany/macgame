using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class DraculaRib : DraculaPart
    {
        public DraculaRib(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player, 5, 35)
        {
        }

        public override bool IsCollected => Game1.StorageState.HasDraculaRib;

        protected override void MarkAsCollected()
        {
            Game1.StorageState.HasDraculaRib = true;
        }

        protected override Color GetPopColor()
        {
            return Color.Gray;
        }
    }
}
