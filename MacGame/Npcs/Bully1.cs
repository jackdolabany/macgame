using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Npcs
{
    public class Bully1 : Bully
    {
        protected override int TextureX => 4;
        protected override int TextureY => 31;
        protected override string BullyMessage => "Fruit and nerds name a better combo!";
        protected override string BullyMessageAfterNerdHit => "Wow you really threw that fruit hard. You could hurt somebody.";
        protected override Color FruitColor => Pallette.Red;

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(3, 5);

        public Bully1(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }
    }
}
