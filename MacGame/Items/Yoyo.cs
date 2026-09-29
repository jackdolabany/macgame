using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    /// <summary>
    /// This item gives Mac a yoyo to swing at enemies. Not the yoyo Mac uses (MacYoyo.cs).
    /// </summary>
    public class Yoyo : Item
    {

        public Yoyo(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(13, 15);
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

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}
