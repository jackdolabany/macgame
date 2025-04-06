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
            // When the player has this render as the powerup the player has.
            // This shows up in the HUD for the player.
            if (_player.CurrentItem == this)
            {
                if (_player.ShotPower == ShotPower.Double)
                {
                    ((StaticImageDisplay)DisplayComponent).Source = firstPowerupSource;
                }
                else if (_player.ShotPower == ShotPower.Charge)
                {
                    ((StaticImageDisplay)DisplayComponent).Source = secondPowerupSource;
                }
            }
            else if (Enabled)
            {
                // Otherwise this is floating for the player to collect, render as the next powerup.
                if (_player.ShotPower == ShotPower.Single)
                {
                    ((StaticImageDisplay)DisplayComponent).Source = firstPowerupSource;
                }
                else
                {
                    ((StaticImageDisplay)DisplayComponent).Source = secondPowerupSource;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
