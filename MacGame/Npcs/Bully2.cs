using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Npcs
{
    public class Bully2 : Bully
    {
        protected override int TextureX => 4;
        protected override int TextureY => 32;
        protected override string BullyMessage => "Hey Mac, get a load of this nerd.";
        protected override string BullyMessageAfterNerdHit => "Not cool, we were having fun but you seriously yeeted that thing.";
        protected override Color FruitColor => Pallette.Orange;
        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(4, 5);

        public Bully2(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }
    }
}
