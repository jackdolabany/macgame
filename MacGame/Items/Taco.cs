using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Items
{
    public class Taco : Item
    {
        public Taco(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = new Rectangle(8 * Game1.TileSize, 2 * Game1.TileSize, 8, 8);
            SetCenteredCollisionRectangle(7, 7);

            IsInChest = false;
        }

        public override void WhenCollected(Player player)
        {
            player.Tacos += 1;

            if(player.Tacos == 10)
            {
                GlobalEvents.FireOneHundredTacosCollected(this, EventArgs.Empty);
            }
            
            // TODO: Play sound
            //SoundManager.PlaySound("CollectItem");
        }
    }
}
