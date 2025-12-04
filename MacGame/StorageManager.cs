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
using System.Collections.Concurrent;

namespace MacGame
{
    public static class StorageManager
    {
        const string SavedGameFileName = "Savegame{0}.sav";

        private static Game1 _game;

        //const string gameSettingsFileName = "GameSettings";
        //const string soundSettingsFileName = "SoundSettings";

        private static Texture2D spinnerTexture;
        static float spinnerRotation;
        private static Rectangle spinnerSourceRect;

        private static bool IsSaving { get; set; }
        private static bool IsLoading { get; set; }

        // Queue for pending saves
        private static ConcurrentQueue<StorageState> saveQueue = new ConcurrentQueue<StorageState>();
        private static Task saveProcessorTask = null;

        /// <summary>
        /// Fade the disk for this amount of time after save.
        /// </summary>
        private const float DiskFadeTimerMax = 0.75f;
        
        private static float _diskfadeTimer = 0f;

        public static bool IsSavingOrLoading
        {
            get
            {
                return IsSaving || IsLoading || !saveQueue.IsEmpty;
            }
        }

        private static void DoneSavingOrLoading()
        {
            IsSaving = false;
            IsLoading = false;
        }

        public static void TrySaveGame(int? saveSlot = null)
        {
            // Clone to be safe since we're going to a background thread.
            StorageState stateToSave = (StorageState)Game1.StorageState.Clone();

            if (saveSlot != null)
            {
                stateToSave.SaveSlot = saveSlot.Value;
            }

            // Add the save to the queue
            saveQueue.Enqueue(stateToSave);

            // Start the save processor if it's not already running
            if (saveProcessorTask == null || saveProcessorTask.IsCompleted)
            {
                saveProcessorTask = Task.Run(ProcessSaveQueue);
            }
        }

        private static async Task ProcessSaveQueue()
        {
            while (saveQueue.TryDequeue(out StorageState stateToSave))
            {
                IsSaving = true;

                try
                {
                    var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var fileName = string.Format(SavedGameFileName, stateToSave.SaveSlot);

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

                    // Testing
                    //System.Threading.Thread.Sleep(3000);

                    // Start to fade the icon away.
                    _diskfadeTimer = DiskFadeTimerMax;
                }
                catch (Exception ex)
                {
                    // Log error but don't crash the game
                    System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                }
            }

            // All saves processed
            IsSaving = false;
        }

        public static void TryLoadGame(int saveSlot)
        {
            IsLoading = true;
            LoadGameWithStorageState(saveSlot);
        }

        private static async void LoadGameWithStorageState(int saveSlot)
        {
            var loadTask = LoadStorageStateForSlot(saveSlot);

            // No async just yet, would need to refactor Game1 to deal with laoding states and possibly update a 
            // loading screen. But this should be pretty quick for now.
            loadTask.Wait();

            var ss = loadTask.Result;

            _game.LoadSavedGame(ss, saveSlot);
            DoneSavingOrLoading();
        }

        public static async Task<StorageState?> LoadStorageStateForSlot(int saveSlot)
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

        public static void TryDeleteGame(int saveSlot)
        {
            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName = string.Format(SavedGameFileName, saveSlot);
            var filePath = appFolderPath + $@"\{Game1.SaveGameFolder}\" + fileName;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void Initialize(Texture2D textures, Game1 game)
        {
            spinnerTexture = textures;
            spinnerSourceRect = Helpers.GetTileRect(9, 4);
            DoneSavingOrLoading();
            _game = game;
        }

        public static void Update(float elapsed)
        {
            if (_diskfadeTimer > 0)
            {
                _diskfadeTimer -= elapsed;
                //spinnerRotation += elapsed * 2f; // Rotate the save icon
                //spinnerRotation = spinnerRotation % (MathHelper.TwoPi);
            }
            else
            {
                spinnerRotation = 0f;
            }
        }

        internal static void Draw(SpriteBatch spriteBatch)
        {
            if (IsSaving || !saveQueue.IsEmpty || _diskfadeTimer > 0)
            {

                var color = Color.White;

                if (!IsSaving && saveQueue.IsEmpty)
                {
                    color = Color.Lerp(Color.White, Color.Transparent, (DiskFadeTimerMax - _diskfadeTimer) / DiskFadeTimerMax);
                }

                spriteBatch.Draw(spinnerTexture, new Vector2(Game1.GAME_X_RESOLUTION - Game1.TileSize - 8, Game1.GAME_Y_RESOLUTION - Game1.TileSize - 8), spinnerSourceRect, color, spinnerRotation, new Vector2(spinnerSourceRect.Width / 2, spinnerSourceRect.Height / 2), 1f, SpriteEffects.None, 0f);
            }
           
            
        }
    }
}
