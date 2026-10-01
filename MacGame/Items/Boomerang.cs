using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    /// <summary>
    /// This item lets Mac throw boomerangs. Not the boomerang Mac throws (MacBoomerang.cs).
    /// </summary>
    public class Boomerang : Item
    {
        public Boomerang(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(10, 14);
            SetWorldLocationCollisionRectangle(8, 8);
            _player = player;
        }

        public override void Collect(Player player)
        {
            EffectsManager.EnemyPop(WorldCenter, 7, Pallette.White, 80);
            player.CurrentItem = this;
            this.Enabled = false;
            base.Collect(player);
        }
    }
}
