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

        private static string GetRuntimeFilePath()
        {
            // Read from bin directory (where the file is copied on build)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "SoundVolumes.json");
        }

        private static string GetSourceFilePath()
        {
            // Save to project root directory for source control
            // Walk up from the bin directory to find the project root
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Navigate up to find the project root (where .csproj file is)
            var projectDir = baseDir;
            while (!string.IsNullOrEmpty(projectDir))
            {
                if (File.Exists(Path.Combine(projectDir, "MacGame.csproj")))
                {
                    return Path.Combine(projectDir, "SoundVolumes.json");
                }
                projectDir = Directory.GetParent(projectDir)?.FullName;
            }

            // Fallback to base directory if project root not found
            return Path.Combine(baseDir, "SoundVolumes.json");
        }

        public static SoundVolumeSettings Load()
        {
            var filePath = GetRuntimeFilePath();

            if (!File.Exists(filePath))
            {
                return new SoundVolumeSettings(); // Return empty if no file exists
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<SoundVolumeSettings>(json);
                return settings ?? new SoundVolumeSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sound volumes: {ex.Message}");
                return new SoundVolumeSettings(); // Return defaults on error
            }
        }

        public void Save()
        {
            try
            {
                var filePath = GetSourceFilePath();
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Sound volumes saved to: {filePath}");
                System.Diagnostics.Debug.WriteLine($"Sound volumes saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving sound volumes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error saving sound volumes: {ex.Message}");
            }
        }
    }
}
