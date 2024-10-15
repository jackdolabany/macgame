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
    public class TacoDoor : OpenCloseDoor
    {

        public TacoDoor(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
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
            return Game1.State.Levels[Game1.CurrentLevel.LevelNumber].Keys.HasTacoKey;
        }

        public override string LockMessage()
        {
            return $"The door is locked.";
        }
    }
}
