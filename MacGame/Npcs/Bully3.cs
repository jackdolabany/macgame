using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Npcs
{
    public class Bully3 : Bully
    {
        protected override int TextureX => 4;
        protected override int TextureY => 33;
        protected override string BullyMessage => "I just love blasting nerds with fruit!";
        protected override string BullyMessageAfterNerdHit => "Brutal, Mac. You took it too far. What's wrong with you?";
        protected override Color FruitColor => Pallette.Yellow;
        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(5, 5);

        public Bully3(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }
    }
}
