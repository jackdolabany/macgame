using Microsoft.Xna.Framework;
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
        public bool HasBeatenGame { get; set; }

        public float TotalElapsedTime { get; set; }

        public bool HasSeenIntroText { get; set; } = false;

        public bool HasBeatenIntroLevel { get; set; } = false;

        public bool HasDancedForDaisy { get; set; } = false;

        public bool HasDraculaHeart { get; set; } = false;

        public bool HasDraculaSkull { get; set; } = false;

        public bool HasDraculaRib { get; set; } = false;

        public bool HasDraculaEye { get; set; } = false;

        public bool HasDraculaTeeth { get; set; } = false;


        /// <param name="saveSlot">1 through 3</param>
        public StorageState(int saveSlot)
        {
            if (saveSlot < 1) throw new Exception("There is no save slot 0. It starts at 1");

            this.SaveSlot = saveSlot;
        }

        public bool IsMapSquareUnblocked(int levelNumber, string mapName, int x, int y)
        {
            if (!this.Levels.ContainsKey(levelNumber)) return false;

            if (!this.Levels[levelNumber].MapNameToUnblockedMapSquares.ContainsKey(mapName)) return false;

            return this.Levels[levelNumber].MapNameToUnblockedMapSquares[mapName].Contains(new Vector2(x, y));
        }

        public void AddUnblockedMapSquare(int levelNumber, string mapName, int x, int y)
        {
            // Add to storage state.
            var mapNameToUnblockedSquares = this.Levels[levelNumber].MapNameToUnblockedMapSquares;
            if (!mapNameToUnblockedSquares.ContainsKey(mapName))
            {
                mapNameToUnblockedSquares.Add(mapName, new List<Vector2>());
            }
            mapNameToUnblockedSquares[mapName].Add(new Vector2(x, y));
        }

        public object Clone()
        {
            var clone = new StorageState(this.SaveSlot);
            clone.HasBeatenGame = this.HasBeatenGame;
            clone.TotalElapsedTime = this.TotalElapsedTime;
            clone.Levels = this.Levels.ToDictionary(kvp => kvp.Key, kvp => (LevelStorageState)kvp.Value.Clone());
            clone.HasSeenIntroText = this.HasSeenIntroText;
            clone.HasBeatenIntroLevel = this.HasBeatenIntroLevel;
            clone.HasDancedForDaisy = this.HasDancedForDaisy;
            clone.HasDraculaHeart = this.HasDraculaHeart;
            clone.HasDraculaSkull = this.HasDraculaSkull;
            clone.HasDraculaRib = this.HasDraculaRib;
            clone.HasDraculaEye = this.HasDraculaEye;
            clone.HasDraculaTeeth = this.HasDraculaTeeth;
            return clone;
        }

        public int GetSockCount()
        {
            return this.Levels.Select(l => l.Value.CollectedSocks.Count).Sum();
        }

        public int GetPercentComplete()
        {
            var sockCount = this.GetSockCount();
            var percentageComplete = (int)System.Math.Round((float)sockCount / (float)Game1.TotalSocks * 100f);
            return percentageComplete;
        }

        public string GetFormattedPlayTime()
        {
            var totalPlayTime = TimeSpan.FromSeconds(this.TotalElapsedTime);
            string totalPlayTimeText;
            if (totalPlayTime > TimeSpan.FromDays(1))
            {
                totalPlayTimeText = $"{totalPlayTime:dd\\:hh\\:mm}";
            }
            else
            {
                totalPlayTimeText = $"{totalPlayTime:hh\\:mm}";
            }
            return totalPlayTimeText;
        }
    }

    public class KeyStoargeState : ICloneable
    {
        public bool HasRedKey { get; set; }
        
        public bool HasGreenKey { get; set; }
        
        public bool HasBlueKey { get; set; }

        /// <summary>
        /// The player doesn't see that they have the Frog key, but if you beat Froggy in a race he'll unlock a door for you.
        /// </summary>
        public bool HasFrogKey { get; set; }

        /// <summary>
        /// The player doesn't know they have the taco key but if they give 100 tacos to the taco
        /// guy he'll unlock a door for them.
        /// </summary>
        public bool HasTacoKey { get; set; }

        public object Clone()
        {
            return new KeyStoargeState
            {
                HasRedKey = this.HasRedKey,
                HasGreenKey = this.HasGreenKey,
                HasBlueKey = this.HasBlueKey,
                HasFrogKey = this.HasFrogKey,
                HasTacoKey = this.HasTacoKey
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
        /// Each sock should have a unique string. Each level should have a number representing the sub world
        /// you entered from the hub world. If there are socks in the hub world that level will be 0.
        /// </summary>
        public HashSet<string> CollectedSocks { get; set; } = new HashSet<string>();

        /// <summary>
        /// Saves if you beat the Frog on slow speed.
        /// </summary>
        public bool HasBeatenFroggySlow { get; set; }

        /// <summary>
        /// Saves if you beat the Frog on medium speed. We don't need to save if you beat
        /// him on Fast because he'll give you a key and that gets saved.
        /// </summary>
        public bool HasBeatenFroggyMedium { get; set; }

        /// <summary>
        /// Any previously blocking spaces that are no longer blocking can be saved here. Examples include
        /// unlocked Keyblocks or blasted BreakBricks.
        /// </summary>
        public Dictionary<string, List<Vector2>> MapNameToUnblockedMapSquares = new Dictionary<string, List<Vector2>>();

        public object Clone()
        {
            var levelStorageState = new LevelStorageState();
            levelStorageState.Keys = (KeyStoargeState)this.Keys.Clone();
            levelStorageState.UnlockedDoors = this.UnlockedDoors.ToHashSet();
            levelStorageState.CollectedSocks = this.CollectedSocks.ToHashSet();
            levelStorageState.HasBeatenFroggySlow = this.HasBeatenFroggySlow;
            levelStorageState.HasBeatenFroggyMedium = this.HasBeatenFroggyMedium;
            levelStorageState.MapNameToUnblockedMapSquares = this.MapNameToUnblockedMapSquares.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
            return levelStorageState;
        }
    }
}
