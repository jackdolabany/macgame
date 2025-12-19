using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.IO.Compression;

namespace MacGame
{
    public class GameSettings
    {
        public bool IsFullScreen { get; set; }
        public CRTMode CRTMode { get; set; }

        public GameSettings()
        {
            IsFullScreen = true;
            CRTMode = CRTMode.None;
        }

        private static string GetSettingsFilePath()
        {
            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appFolderPath, Game1.SaveGameFolder, "Settings.dat");
        }

        public static GameSettings Load()
        {
            var filePath = GetSettingsFilePath();

            if (!File.Exists(filePath))
            {
                return new GameSettings(); // Return defaults if no file exists
            }

            try
            {
                using (var savedFile = File.Open(filePath, FileMode.Open))
                {
                    // Unzip savedFile
                    using (var zipStream = new GZipStream(savedFile, CompressionMode.Decompress))
                    {
                        using (var decompressedStream = new MemoryStream())
                        {
                            zipStream.CopyTo(decompressedStream);
                            decompressedStream.Position = 0;
                            var bytes = decompressedStream.ToArray();
                            var json = Encoding.UTF8.GetString(bytes);
                            var settings = JsonConvert.DeserializeObject<GameSettings>(json);
                            return settings ?? new GameSettings();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return new GameSettings(); // Return defaults on error
            }
        }

        public void Save()
        {
            try
            {
                var filePath = GetSettingsFilePath();

                // Convert to Json using JSON.NET
                var json = JsonConvert.SerializeObject(this);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Zip compress the bytes
                using (var compressedStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        zipStream.Write(bytes, 0, bytes.Length);
                    }
                    bytes = compressedStream.ToArray();
                }

                // Ensure directory exists
                var file = new FileInfo(filePath);
                file.Directory!.Create();

                // Write file
                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
