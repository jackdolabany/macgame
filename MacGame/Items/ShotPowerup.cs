using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// This item powers up Mac's shot in his spaceship.
    /// </summary>
    public class ShotPowerup : Item
    {

        Rectangle firstPowerupSource;
        Rectangle secondPowerupSource;
       
        public ShotPowerup(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;

            firstPowerupSource = Helpers.GetTileRect(1, 5);
            secondPowerupSource = Helpers.GetTileRect(2, 5);

            image.Source = firstPowerupSource;
            SetWorldLocationCollisionRectangle(8, 8);
            _player = player;
            IsReenabledOnceOffScreen = false;
            IsInChest = false;
      
        }

        public override void WhenCollected(Player player)
        {
            EffectsManager.EnemyPop(WorldCenter, 7, Color.White, 80);
            player.CurrentItem = this;
            this.Enabled = false;

            _player.HandleShotPowerupCollected();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled && _player.ShotPower == ShotPower.Double)
            {
                ((StaticImageDisplay)DisplayComponent).Source = secondPowerupSource;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
