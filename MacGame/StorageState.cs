using System;
using System.Collections.Generic;
using System.Linq;

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

        public Dictionary<int, LevelStorageState> Levels { get; set; } = new Dictionary<int, LevelStorageState>();

        /// <summary>
        ///  Set to true if you beat the game. Maybe we'll display a star or something on your save file.
        /// </summary>
        public bool HasBeatedGame { get; set; }

        public float TotalElapsedTime { get; set; }

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
            clone.HasBeatedGame = this.HasBeatedGame;
            clone.TotalElapsedTime = this.TotalElapsedTime;
            clone.Levels = this.Levels.ToDictionary(kvp => kvp.Key, kvp => (LevelStorageState)kvp.Value.Clone());
            return clone;
        }
    }

    public class KeyStoargeState : ICloneable
    {
        public bool HasRedKey { get; set; }
        public bool HasGreenKey { get; set; }
        public bool HasBlueKey { get; set; }

        public object Clone()
        {
            return new KeyStoargeState
            {
                HasRedKey = this.HasRedKey,
                HasGreenKey = this.HasGreenKey,
                HasBlueKey = this.HasBlueKey
            };
        }
    }

    /// <summary>
    /// Savable state for a given level. A level may span multiple maps.
    /// </summary>
    public class LevelStorageState : ICloneable
    {
        public KeyStoargeState Keys { get; set; } = new KeyStoargeState();
        public HashSet<string> UnlockedDoors { get; set; } = new HashSet<string>();

        /// <summary>
        /// Each coin should have a unique string. Each level should have a number representing the sub world
        /// you entered from the hub world. If there are coins in the hub world that level will be 0.
        /// </summary>
        public HashSet<string> CollectedCoins { get; set; } = new HashSet<string>();

        public object Clone()
        {
            return new LevelStorageState
            {
                Keys = (KeyStoargeState)this.Keys.Clone(),
                UnlockedDoors = this.UnlockedDoors.ToHashSet(),
                CollectedCoins = this.CollectedCoins.ToHashSet()
            };
        }
    }
}
