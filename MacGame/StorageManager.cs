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
        const string SavedGameFileName = "Savegame{0}.sav";

        private static Game1 _game;

        //const string gameSettingsFileName = "GameSettings";
        //const string soundSettingsFileName = "SoundSettings";

        private static Texture2D spinnerTexture;
        static float spinnerRotation;
        private static Rectangle spinnerSourceRect;

        private static bool IsSaving { get; set; }
        private static bool IsLoading { get; set; }

        public static bool IsSavingOrLoading
        {
            get
            {
                return IsSaving || IsLoading;
            }
        }

        private static void DoneSavingOrLoading()
        {
            IsSaving = false;
            IsLoading = false;
        }

        public static void TrySaveGame(int? saveSlot = null)
        {
            IsSaving = true;

            // Clone to be safe since we're going to a background thread.
            StorageState stateToSave = (StorageState)Game1.State.Clone();

            if (saveSlot != null)
            {
                stateToSave.SaveSlot = saveSlot.Value;
            }

            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName = string.Format(SavedGameFileName, stateToSave.SaveSlot);

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

                // Testing
                //System.Threading.Thread.Sleep(3000);

                DoneSavingOrLoading();
            });

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
            if (!IsSavingOrLoading) return; // save some clock cycles, this is by far the most common case.

            // spinnerRotation += elapsed * 1;
        }

        internal static void Draw(SpriteBatch spriteBatch)
        {
            if (!IsSavingOrLoading) return;
            //string savingText = "";
            //if (IsSaving)
            //{
            //    savingText = "Saving...";
            //}
            //else if (IsLoading)
            //{
            //    savingText = "Loading...";
            //}
            // spriteBatch.DrawString(Game1.Font, savingText, new Vector2(Game1.GAME_X_RESOLUTION / 2 - 56, Game1.GAME_Y_RESOLUTION - 14), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(spinnerTexture, new Vector2(Game1.GAME_X_RESOLUTION - Game1.TileSize - 8, Game1.GAME_Y_RESOLUTION - Game1.TileSize - 8), spinnerSourceRect, Color.White, spinnerRotation, new Vector2(spinnerSourceRect.Width / 2, spinnerSourceRect.Height / 2), 1f, SpriteEffects.None, 0f);
        }
    }
}
