using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MacGame
{

    /// <summary>
    /// The state of the job you do for the mob.
    /// </summary>
    public enum JobState
    {
        NotAccepted,
        Accepted,
        CarDamaged,
        CarDestroyed,
        SockCollected
    }

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

        /// <summary>
        /// Only for level 3 where you accept a job from the mob.
        /// </summary>
        public JobState JobState { get; set; } = JobState.NotAccepted;

        /// <summary>
        /// Only for the Dracula boss. Don't want to hear his conversation each time you fight him.
        /// </summary>
        public bool HasHeardDraculaConversation = false;

        /// <summary>
        /// Tracks how many times the player has talked to the Chatterbox NPC in the current level session.
        /// </summary>
        public int ChatterboxConversationCount = 0;

        /// <summary>
        /// If there is a murderer in the level, we track their health here so it persists across map changes.
        /// </summary>
        public int? MurdererHealth = null;

        /// <summary>
        /// Track the state of Crystal Switches that control the orange and blue blocks in LevelState.
        /// </summary>
        public bool CrystalSwitchIsOrange = true;

        public bool CrystalSwitchIsBlue
        {
            get
            {
                return !CrystalSwitchIsOrange;
            }
            set
            {
                CrystalSwitchIsOrange = !value;
            }
        }

        public void Reset()
        {
            HubDoorNameYouCameFrom = "";
            MapNameToCollectedTacos.Clear();
            WaterHeight = WaterHeight.High;
            JobState = JobState.NotAccepted;
            HasHeardDraculaConversation = false;
            ChatterboxConversationCount = 0;
            MurdererHealth = null;
            CrystalSwitchIsOrange = true;
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
