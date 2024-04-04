using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Door : GameObject
    {
        public string Name = "";
        public string GoToMap = "";
        public string GoToDoorName = "";

        /// <summary>
        /// If the door is coming from a hub world and going to a sub world display the hints from that sub world.
        /// </summary>
        public bool IsToSubworld = false;

        public int CoinsNeeded;

        public Door(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            this.CollisionRectangle = new Rectangle(-4, -16, 8, 16);
            var sid = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), new Rectangle(0, 72, 8, 16));
            this.DisplayComponent = sid;

        }
    }
}
