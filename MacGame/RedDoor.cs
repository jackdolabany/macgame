using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame
{

    /// <summary>
    ///  A door that opens and closes. As opposed to a doorway that is always open.
    /// </summary>
    public class RedDoor : OpenCloseDoor
    {
        
        public RedDoor(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {
       
        }

        public override Rectangle DoorImageTextureSourceRectangle => 
            new Rectangle(10 * Game1.TileSize, 7 * Game1.TileSize, 16, 16);

        public override bool IsInitiallyLocked
        {
            get
            {
                return true;
            }
        }

        public override bool CanPlayerUnlock(Player player)
        {
            return player.HasRedKey;
        }

        public override string LockMessage()
        {
            return $"You need the red key to unlock this door.";
        }
    }
}
