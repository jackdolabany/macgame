using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class DraculaHeart : DraculaPart
    {
        public DraculaHeart(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player, 3, 35)
        {
        }

        public override bool IsCollected => Game1.StorageState.HasDraculaHeart;

        protected override void MarkAsCollected()
        {
            Game1.StorageState.HasDraculaHeart = true;
        }

        protected override Color GetPopColor()
        {
            return Color.Red;
        }
    }
}
