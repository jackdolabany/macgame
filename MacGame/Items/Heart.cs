using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    public class Heart : Item
    {
        public Heart(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(1, 2);
            SetWorldLocationCollisionRectangle(8, 8);
        }

        public override void WhenCollected(Player player)
        {
            this.Enabled = false;
            player.Health += 1;
            if (player.Health > Player.MaxHealth)
            {
                player.Health = Player.MaxHealth;
            }
            
            EffectsManager.EnemyPop(WorldCenter, 7, Color.Red, 80);
        }

        public override void PlayCollectedSound()
        {
            SoundManager.PlaySound("Health");
        }
    }
}
