using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using General;

namespace ConfigBuilder
{
    class Program
    {

        /// <summary>
        /// This is run on build of the MacGame project. It runs and reads plain text .json files and copies them with obfuscated names to 
        /// .gz compressed .dat files.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                // Get the MacGame project directory (one level up from ConfigBuilder)
                var configBuilderDir = AppDomain.CurrentDomain.BaseDirectory;
                var solutionRoot = Path.GetFullPath(Path.Combine(configBuilderDir, "..", "..", "..", ".."));
                var macGameProjectDir = Path.Combine(solutionRoot, "MacGame");

                Console.WriteLine($"Solution root: {solutionRoot}");
                Console.WriteLine($"MacGame project: {macGameProjectDir}");
                Console.WriteLine();

                // Process PlayerSettings
                ProcessConfigFile(macGameProjectDir, "PlayerSettings");

                // Process SoundVolumes
                ProcessConfigFile(macGameProjectDir, "SoundVolumes");

                Console.WriteLine();
                Console.WriteLine("Config files built successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        static void ProcessConfigFile(string projectDir, string fileName)
        {
            var jsonPath = Path.Combine(projectDir, $"{fileName}.json");

            // Use obfuscated filename for .dat file
            var datFileName = ConfigSettings.ConfigToDatMapping.ContainsKey(fileName)
                ? ConfigSettings.ConfigToDatMapping[fileName]
                : fileName;
            var datPath = Path.Combine(projectDir, $"{datFileName}.dat");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"Warning: {jsonPath} not found, skipping...");
                return;
            }

            try
            {
                // Read the JSON file
                var json = File.ReadAllText(jsonPath);
                var bytes = Encoding.UTF8.GetBytes(json);

                // GZip compress the bytes (same logic as ConfigFileManager)
                using (var compressedStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        zipStream.Write(bytes, 0, bytes.Length);
                    }
                    bytes = compressedStream.ToArray();
                }

                // Write the compressed .dat file
                File.WriteAllBytes(datPath, bytes);

                Console.WriteLine($"✓ Created {datFileName}.dat from {fileName}.json ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to process {fileName}: {ex.Message}");
                throw;
            }
        }
    }
}
