using System.Collections.Generic;

namespace MacGame
{
    /// <summary>
    /// This represents a savable game file.
    /// </summary>
    public class StorageState
    {
        /// <summary>
        /// Which save slot this is.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Each coin should have a unique index 1-x. Each level should have a number representing the sub world
        /// you entered from the hub world. If there are coins in the hub world they can be 0.
        /// </summary>
        public Dictionary<int, List<int>> LevelsToCoins = new Dictionary<int, List<int>>();

        public Dictionary<int, int> MaxTacosPerLevel = new Dictionary<int, int>();

        /// <summary>
        ///  Set to true if you beat the game. Maybe we'll display a star or something on your save file.
        /// </summary>
        public bool HasBeatedGame { get; set; }
    }
}
