using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using General;

namespace MacGame
{
    /// <summary>
    /// Manages loading and saving configuration files.
    /// Load: Checks solution-level .json first, falls back to bin-level .dat file
    /// Save: Creates both .json (for editing) and .dat (for distribution) at solution level
    /// </summary>
    public static class ConfigFileManager
    {
        /// <summary>
        /// Indicates whether source .json files exist (development mode).
        /// Cached on first access to avoid repeated file system checks.
        /// </summary>
        public static bool SourceFilesExist { get; private set; }

        static ConfigFileManager()
        {
            // Check if we're in development by looking for PlayerSettings.json in the solution
            var sourceFilePath = GetSourceFilePath("PlayerSettings");
            SourceFilesExist = File.Exists(sourceFilePath);
        }
        /// <summary>
        /// Load a configuration object from disk.
        /// Always loads from bin-level .dat file (initial state).
        /// .json files are for editing only - changes are picked up via file watching.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="fileNameWithoutExtension">Base filename (e.g., "PlayerSettings")</param>
        /// <returns>Deserialized object or default if file doesn't exist</returns>
        public static T LoadConfig<T>(string fileNameWithoutExtension) where T : class, new()
        {

            // Always load from bin-level .dat file
            var datFileName = ConfigSettings.ConfigToDatMapping[fileNameWithoutExtension];
            var binDatPath = GetBinFilePath(datFileName);

            using (var fileStream = File.Open(binDatPath, FileMode.Open))
            {
                using (var zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (var decompressedStream = new MemoryStream())
                    {
                        zipStream.CopyTo(decompressedStream);
                        decompressedStream.Position = 0;
                        var bytes = decompressedStream.ToArray();
                        var json = Encoding.UTF8.GetString(bytes);
                        var config = JsonConvert.DeserializeObject<T>(json);
                        return config!;
                    }
                }
            }
        }

        /// <summary>
        /// Save a configuration object to disk.
        /// Only saves to .json file at solution level for editing.
        /// .dat files are regenerated at build time by ConfigBuilder.
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="fileNameWithoutExtension">Base filename (e.g., "PlayerSettings")</param>
        /// <param name="config">The object to save</param>
        public static void SaveConfig<T>(string fileNameWithoutExtension, T config) where T : class
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Save plain JSON to solution level for editing
            var jsonPath = GetSourceFilePath(fileNameWithoutExtension);
            File.WriteAllText(jsonPath, json);
        }

        /// <summary>
        /// Get the path to a config file at the solution/project level.
        /// </summary>
        public static string GetSourceFilePath(string fileNameWithoutExtension)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            return Path.Combine(projectRoot, $"{fileNameWithoutExtension}.json");
        }

        /// <summary>
        /// Get the path to a config file in the bin/output directory.
        /// </summary>
        private static string GetBinFilePath(string fileNameWithoutExtension)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{fileNameWithoutExtension}.dat");
        }
    }
}
