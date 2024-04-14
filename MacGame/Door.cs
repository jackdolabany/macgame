using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public abstract class Door : GameObject
    {
        public string Name = "";
        public string GoToMap = "";
        public string GoToDoorName = "";

        protected Player _player;

        public Door(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            this.CollisionRectangle = new Rectangle(-4, -16, 8, 16);

            _player = player;
        }

        public virtual void PlayerTriedToOpen(Player player)
        {
            GlobalEvents.FireDoorEntered(this, this.GoToMap, this.GoToDoorName, this.Name);
        }
    }
}
