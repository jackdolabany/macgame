using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General
{
    public static class ConfigSettings
    {
        // Maps config names to their obfuscated .dat filenames
        public static readonly Dictionary<string, string> ConfigToDatMapping = new Dictionary<string, string>
        {
            { "PlayerSettings", "gamedata1" },
            { "SoundVolumes", "gamedata2" }
        };
    }
}
