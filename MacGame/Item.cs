using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame
{
    public abstract class Item : GameObject
    {
        public Item(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        { 
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
        }

        public virtual void Collect(Player player)
        {
            WhenCollected(player);
            this.Enabled = false;
        }

        public abstract void WhenCollected(Player player);

    }
}
