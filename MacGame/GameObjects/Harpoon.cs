using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// Show from Mac's submarine.
    /// </summary>
    public class Harpoon : GameObject
    {
        private Player _player;

        public Harpoon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(15, 14);
            SetCenteredCollisionRectangle(8, 2);
            IsAffectedByGravity = false;
            isTileColliding = false;
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    ReturnHarpoon();
                }

                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldLocation);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    this.Break();
                }
            }

            Flipped = this.velocity.X < 0;

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void ReturnHarpoon()
        {
            this.Enabled = false;
            _player.Harpoons.ReturnObject(this);
        }

        public void Break()
        {
            ReturnHarpoon();
            EffectsManager.SmallEnemyPop(this.WorldCenter);
            SoundManager.PlaySound("Break");
        }
    }
}
