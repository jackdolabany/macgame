using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Items
{
    public class Taco : Item
    {

        int x;
        int y;

        public Taco(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            x = cellX;
            y = cellY;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(8, 2);
            SetWorldLocationCollisionRectangle(7, 7);
        }

        public override void WhenCollected(Player player)
        {
            player.Tacos += 1;

            this.Enabled = false;

            // Add to the collection so it stays collected.
            Game1.TacoCollected(Game1.CurrentLevel.Name, x, y);
        }

        public override void PlayCollectedSound()
        {
            SoundManager.PlaySound("TacoCollected", 0.3f);
        }
    }
}
