using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MacGame
{
    public class SoundVolumeSettings
    {
        public Dictionary<string, int> SoundVolumes { get; set; }

        public SoundVolumeSettings()
        {
            SoundVolumes = new Dictionary<string, int>();
        }

        public static SoundVolumeSettings Load()
        {
            return ConfigFileManager.LoadConfig<SoundVolumeSettings>("SoundVolumes");
        }

        public void Save()
        {
            ConfigFileManager.SaveConfig("SoundVolumes", this);
        }
    }
}
