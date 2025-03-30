using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    public class Apples : Item
    {

        public Apples(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(14, 0);
            SetWorldLocationCollisionRectangle(8, 8);
            _player = player;
            IsReenabledOnceOffScreen = true;
        }

        public override void WhenCollected(Player player)
        {
            EffectsManager.EnemyPop(WorldCenter, 7, Color.White, 80);
            player.CurrentItem = this;
            this.Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}
