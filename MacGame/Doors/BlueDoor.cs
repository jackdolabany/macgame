using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Doors
{

    /// <summary>
    ///  A door that opens and closes. As opposed to a doorway that is always open.
    /// </summary>
    public class BlueDoor : OpenCloseDoor
    {

        public BlueDoor(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

        }

        public override Rectangle DoorImageTextureSourceRectangle => TileEngine.Helpers.GetBigTileRect(0, 5);

        public override bool IsInitiallyLocked
        {
            get
            {
                return true;
            }
        }

        public override bool CanPlayerUnlock(Player player)
        {
            return Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].Keys.HasBlueKey;
        }

        public override string LockMessage()
        {
            return $"You need the blue key.";
        }
    }
}
