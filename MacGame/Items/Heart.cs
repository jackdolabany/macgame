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
    public class Heart : Item
    {
        public Heart(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = new Rectangle(8, 16, 8, 8);
            SetCenteredCollisionRectangle(8, 8);
        }

        public override void WhenCollected(Player player)
        {
            player.Health += 1;
            if (player.Health > Player.MaxHealth)
            {
                player.Health = Player.MaxHealth;
            }
            SoundManager.PlaySound("health");
            EffectsManager.EnemyPop(WorldCenter, 7, Color.Red, 20);
        }
    }
}
