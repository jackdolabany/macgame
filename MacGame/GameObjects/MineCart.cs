using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame
{
    public class MineCart : GameObject
    {
        private Player _player;

        public MineCart(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures, Helpers.GetTileRect(2, 9));
            this.DisplayComponent = image;
            SetCenteredCollisionRectangle(8, 8);
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (Enabled)
            {
                if (this.CollisionRectangle.Contains(_player.WorldCenter))
                {
                    this.Enabled = false;
                    _player.IsInMineCart = true;
                    SoundManager.PlaySound("PowerUp");
                }
            }

        }

    }
}
