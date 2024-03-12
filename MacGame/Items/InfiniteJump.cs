using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TileEngine;
using static System.Net.Mime.MediaTypeNames;

namespace MacGame.Items
{
    public class InfiniteJump : Item
    {

        public InfiniteJump(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = new Rectangle(11 * TileMap.TileSize, 0, TileMap.TileSize, TileMap.TileSize);
            SetCenteredCollisionRectangle(8, 8);
            _player = player;
            IsReenabled = true;
        }

        public override void WhenCollected(Player player)
        {
            EffectsManager.EnemyPop(WorldCenter, 7, Color.White, 20);
            player.CurrentItem = this;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}
