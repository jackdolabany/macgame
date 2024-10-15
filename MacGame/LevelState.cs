using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MacGame
{
    /// <summary>
    /// This class holds non-savable state that needs to be kept for a level. While traversing different map files within a level this state is maintained. It's 
    /// only when you go back to the hub or enter a new level that this state would be reset.
    /// </summary>
    public class LevelState
    {
        /// <summary>
        /// When Mac enters a level we track which door he came from so we can send him back if he dies (so sad!).
        /// </summary>
        public string HubDoorNameYouCameFrom { get; set; } = "";

        /// <summary>
        /// For the given level/world, track which tacos were picked up. 
        /// we need this so that a collected taco stays collected if you go
        /// into a door and back.
        /// Taco locations are stored in the Vector corresponding to the initial tile location on the map.
        /// </summary>
        public static Dictionary<string, List<Vector2>> MapNameToCollectedTacos = new Dictionary<string, List<Vector2>>();

        public WaterHeight WaterHeight { get; set; } = WaterHeight.High;

        public void Reset()
        {
            HubDoorNameYouCameFrom = "";
            MapNameToCollectedTacos.Clear();
            WaterHeight = WaterHeight.High;
        }
    }

    /// <summary>
    /// Used by any maps where you can control the water height.
    /// </summary>
    public enum WaterHeight
    {
        High,
        Medium,
        Low
    }

}
