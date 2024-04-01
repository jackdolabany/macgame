using System.Collections.Generic;

namespace MacGame
{
    public class NextLevelInfo
    {
        public string MapName = "";
        public Dictionary<int, string> CoinHints = new Dictionary<int, string>();
        public int LevelNumber = 0;
        public string Description = "";
    }
}
