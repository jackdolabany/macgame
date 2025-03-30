using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    public class InfiniteJump : Item
    {

        public InfiniteJump(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(11, 0);
            SetWorldLocationCollisionRectangle(8, 8);
            _player = player;
            IsReenabledOnceOffScreen = true;
        }

        public override void WhenCollected(Player player)
        {
            this.Enabled = false;
            EffectsManager.EnemyPop(WorldCenter, 7, Color.White, 80);
            player.CurrentItem = this;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}
