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
    public class FrogDoor : OpenCloseDoor
    {

        public FrogDoor(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player)
        {

        }

        public override bool IsInitiallyLocked
        {
            get
            {
                return true;
            }
        }

        public override bool CanPlayerUnlock(Player player)
        {
            return Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].Keys.HasFrogKey;
        }

        public override string LockMessage()
        {
            return $"The door is locked.";
        }
    }
}
