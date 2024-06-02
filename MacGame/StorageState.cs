using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MacGame
{
    /// <summary>
    /// This represents a savable game file.
    /// </summary>
    public class StorageState: ICloneable
    {
        /// <summary>
        /// Which save slot this is, 1 through 3.
        /// </summary>
        public int SaveSlot { get; set; }

        /// <summary>
        /// Each coin should have a unique index 1-x. Each level should have a number representing the sub world
        /// you entered from the hub world. If there are coins in the hub world they can be 0.
        /// </summary>
        public Dictionary<int, HashSet<int>> LevelsToCoins = new Dictionary<int, HashSet<int>>();

        public Dictionary<int, int> MaxTacosPerLevel = new Dictionary<int, int>();

        public Dictionary<int, HashSet<string>> UnlockedDoors = new Dictionary<int, HashSet<string>>();

        /// <summary>
        ///  Set to true if you beat the game. Maybe we'll display a star or something on your save file.
        /// </summary>
        public bool HasBeatedGame { get; set; }

        /// <param name="saveSlot">1 through 3</param>
        public StorageState(int saveSlot)
        {
            if (saveSlot < 1) throw new Exception("There is no save slot 0. It starts at 1");

            if (saveSlot > 3) throw new Exception("Only 3 save slots.");
            this.SaveSlot = saveSlot;
        }

        public object Clone()
        {
            var clone = new StorageState(this.SaveSlot);
            clone.LevelsToCoins = new Dictionary<int, HashSet<int>>(this.LevelsToCoins);
            clone.MaxTacosPerLevel = new Dictionary<int, int>(this.MaxTacosPerLevel);
            clone.UnlockedDoors = new Dictionary<int, HashSet<string>>(this.UnlockedDoors);
            clone.HasBeatedGame = this.HasBeatedGame;
            return clone;
        }
    }
}
