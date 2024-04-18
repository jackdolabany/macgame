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

namespace MacGame
{
    public static class StorageManager
    {
        //const string GameFolderName = "Macs_Adventure";
        const string SavedGameFileName = "Savegame{0}.sav";
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
            //Game1.LastSaveState = Game1.State.Clone();
            var stateToSave = Game1.State.Clone();

            //XmlSerializer serializer = new XmlSerializer(typeof(StorageState));

            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName = string.Format(SavedGameFileName, saveSlot);

            Task.Run(() =>
            {
                //using (var saveFile = File.Create(appFolderPath + @"\" + fileName))
                //{
                //    serializer.Serialize(saveFile, stateToSave);
                //    DoneSavingOrLoading();
                //}
                

                // TEsting
                System.Threading.Thread.Sleep(2000); 
                DoneSavingOrLoading();



            });

        }

        //public static void TryLoadGame(int saveSlot)
        //{
        //    IsLoading = true;
        //    LoadCurrentSlotStorageState(saveSlot);
        //}

        //private static async void LoadCurrentSlotStorageState(int saveSlot)
        //{
        //    var ss = await LoadSlotStorageState(saveSlot);
        //    LevelManager.CurrentGame.LoadSavedGame(ss);
        //    DoneSavingOrLoading();
        //}

        //private static async Task<StorageState> LoadSlotStorageState(int saveSlot)
        //{
        //    var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //    var fileName = string.Format(SavedGameFileName, saveSlot);
        //    var fullFilePath = appFolderPath + @"\" + fileName;

        //    return await System.Threading.Tasks.Task.Run(() =>
        //    {
        //        if (!File.Exists(fullFilePath)) return null;
        //        using (var savedFile = File.Open(fullFilePath, FileMode.Open))
        //        {
        //            var serializer = new XmlSerializer(typeof(StorageState));
        //            var ss = serializer.Deserialize(savedFile) as StorageState;
        //            DoneSavingOrLoading();
        //            return ss;
        //        }
        //    });
        //}

        public static void Initialize(Texture2D textures)
        {
            // StorageManager.players = players;
            spinnerTexture = textures;
            spinnerSourceRect = new Rectangle(9 * Game1.TileSize, 4 * Game1.TileSize, Game1.TileSize, Game1.TileSize);
            DoneSavingOrLoading();
        }

        public static void Update(float elapsed)
        {
            if (!IsSavingOrLoading) return; // save some clock cycles, this is by far the most common case.

            //spinnerRotation += 2f * elapsed;
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
