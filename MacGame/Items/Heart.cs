using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    public class Heart : Item
    {
        public Heart(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(1, 2);
            SetCenteredCollisionRectangle(8, 8);
        }

        public override void WhenCollected(Player player)
        {
            player.Health += 1;
            if (player.Health > Player.MaxHealth)
            {
                player.Health = Player.MaxHealth;
            }
            SoundManager.PlaySound("health");
            EffectsManager.EnemyPop(WorldCenter, 7, Color.Red, 20);
        }
    }
}
