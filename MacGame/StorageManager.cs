using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using TileEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using MacGame;
using Newtonsoft.Json;
using System.IO.Compression;

namespace MacGame
{
    public static class StorageManager
    {
        //const string GameFolderName = "Macs_Adventure";
        const string SavedGameFileName = "Savegame{0}.sav";

        private static Game1 _game;

        //const string gameSettingsFileName = "GameSettings";
        //const string soundSettingsFileName = "SoundSettings";

        // private static Player[] players;
        private static Texture2D spinnerTexture;
        static float spinnerRotation;
        private static Rectangle spinnerSourceRect;

        //public static StorageManagerMessage Message { get; internal set; }

        private static bool IsSaving { get; set; }
        private static bool IsLoading { get; set; }

        public static bool IsSavingOrLoading
        {
            get
            {
                return IsSaving || IsLoading;
            }
        }

        //public static string ContentPath
        //{
        //    get
        //    {
        //        var path = Directory.GetCurrentDirectory();
        //        var searchPattern = @"\MacGame\";
        //        path = path.Substring(0, path.LastIndexOf(searchPattern) + searchPattern.Length);
        //        path += @"MacGame\Content";
        //        return path;
        //    }
        //}

        private static void DoneSavingOrLoading()
        {
            IsSaving = false;
            IsLoading = false;
        }

        public static void TrySaveGame(int saveSlot)
        {
            IsSaving = true;

            // Clone to be safe since we're going to a background thread.
            var stateToSave = Game1.State.Clone();

            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName = string.Format(SavedGameFileName, saveSlot);

            Task.Run(() =>
            {
                // Convert to Json using JSON.NET and write the file.
                var json = JsonConvert.SerializeObject(stateToSave);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Zip compress the bytes.
                using (var compressedStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        zipStream.Write(bytes, 0, bytes.Length);
                    }
                    bytes = compressedStream.ToArray();
                }

                var filePath = appFolderPath + $@"\{Game1.SaveGameFolder}\" + fileName;

                var file = new FileInfo(filePath);
                file.Directory!.Create();
                File.WriteAllBytes(filePath, bytes);
                
                DoneSavingOrLoading();
            });

        }

        public static void TryLoadGame(int saveSlot)
        {
            IsLoading = true;
            LoadCurrentSlotStorageState(saveSlot);
        }

        private static async void LoadCurrentSlotStorageState(int saveSlot)
        {
            var ss = await LoadSlotStorageState(saveSlot);
            _game.LoadSavedGame(ss);
            DoneSavingOrLoading();
        }

        private static async Task<StorageState?> LoadSlotStorageState(int saveSlot)
        {
            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName = string.Format(SavedGameFileName, saveSlot);
            var filePath = appFolderPath + $@"\{Game1.SaveGameFolder}\" + fileName;

            return await Task.Run(() =>
            {
                if (!File.Exists(filePath)) return null;

                StorageState? ss = null;

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
                            ss = JsonConvert.DeserializeObject<StorageState>(json);
                        }
                    }
                }

                DoneSavingOrLoading();
                return ss;
            });
        }

        public static void Initialize(Texture2D textures, Game1 game)
        {
            spinnerTexture = textures;
            spinnerSourceRect = new Rectangle(9 * Game1.TileSize, 4 * Game1.TileSize, Game1.TileSize, Game1.TileSize);
            DoneSavingOrLoading();
            _game = game;
        }

        public static void Update(float elapsed)
        {
            if (!IsSavingOrLoading) return; // save some clock cycles, this is by far the most common case.
        }

        internal static void Draw(SpriteBatch spriteBatch)
        {
            if (!IsSavingOrLoading) return;
            string savingText = "";
            if (IsSaving)
            {
                savingText = "Saving...";
            }
            else if (IsLoading)
            {
                savingText = "Loading...";
            }
            spriteBatch.DrawString(Game1.Font, savingText, new Vector2(Game1.GAME_X_RESOLUTION - 56, Game1.GAME_Y_RESOLUTION - 14), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(spinnerTexture, new Vector2(Game1.GAME_X_RESOLUTION - 10, Game1.GAME_Y_RESOLUTION - 8), spinnerSourceRect, Color.White, spinnerRotation, new Vector2(spinnerSourceRect.Width / 2, spinnerSourceRect.Height / 2), 1f, SpriteEffects.None, 0f);
        }

        //internal static bool IsReady()
        //{
        //    return true;
        //}

        //internal static void TryLoadAllSlots()
        //{
        //    Message = null;
        //    IsLoading = true;
        //    LoadAllSlots();
        //}

        //private static async void LoadAllSlots()
        //{
        //    var returnArray = new StorageState[4];
        //    for (int i = 1; i <= 4; i++)
        //    {
        //        var ss = await LoadSlotStorageState(i);
        //        returnArray[i - 1] = ss;
        //    }
        //    Message = new StorageManagerMessage();
        //    Message.Payload = returnArray;
        //    DoneSavingOrLoading();
        //}

        //internal static void TrySaveSoundSettings()
        //{
        //    // Build the settings file
        //    var soundSettings = new SoundSettings();
        //    soundSettings.MusicVolume = SoundManager.MusicVolume;
        //    soundSettings.SoundEffectVolume = SoundManager.SoundEffectVolume;
        //    soundSettings.SoundEffectNames = SoundManager.Sounds.Select(s => s.Key).ToArray();
        //    soundSettings.SoundEffectVolumes = SoundManager.Sounds.Select(s => s.Value.Volume).ToArray();
        //    soundSettings.SongNames = SoundManager.Songs.Select(s => s.Key).ToArray();
        //    soundSettings.SongVolumes = SoundManager.Songs.Select(s => s.Value.Volume).ToArray();

        //    string path = ContentPath + "\\Sounds\\" + soundSettingsFileName;

        //    // Save to the content directory. The .xnb file will be loaded next time you compile.
        //    IsSaving = true;
        //    System.Threading.Tasks.Task.Run(() =>
        //    {
        //        using (var sr = new StreamWriter(path, false))
        //        {
        //            var serializer = new XmlSerializer(typeof(SoundSettings));
        //            serializer.Serialize(sr, soundSettings);
        //        }
        //        DoneSavingOrLoading();
        //    });

        //}
    }
}
