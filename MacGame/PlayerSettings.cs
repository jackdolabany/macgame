using System;
using System.IO;
using Newtonsoft.Json;

namespace MacGame
{
    public static class PlayerSettings
    {
        private static FileSystemWatcher? _watcher;
        private static string _filePath = ""; // Path in the output/build directory
        private static string _sourceFilePath = ""; // Path in the source directory
        private static DateTime _lastReloadTime = DateTime.MinValue;
        private static readonly object _lockObject = new object();


        /// <summary>
        /// The max speed the character can run in pixels per second.
        /// </summary>
        public static float MaxRunSpeed { get; private set; }
        public static float MaxWalkSpeed { get; private set; }

        public static float RunAcceleration { get; private set; }

        public static float RunDeceleration { get; private set; }

        /// <summary>
        /// If you are running in one direction, the speed at which you can bring your 
        /// speed to zero on a turn.
        /// </summary>
        public static float TurnSpeed { get; private set; }

        /// <summary>
        /// The exact height you can jump.
        /// </summary>
        public static float JumpHeight { get; private set; }

        public static float EarthGravity { get; private set; }
        public static float MoonGravity { get; private set; }
        public static float WaterGravity { get; private set; }

        /// <summary>
        /// The amount of time it takes for you to reach the JumpHeight.
        /// </summary>
        public static float JumpDuration { get; private set; }

        /// <summary>
        /// How fast you can increase vertical speed while in the air.
        /// </summary>
        public static float AirAcceleration { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static float AirControl { get; private set; }

        public static float AirBreak { get; private set; }

        /// <summary>
        /// Between 1.0 and 0.0. The percentage of the jump you can cut off by releasing the jump button early.
        /// </summary>
        public static float JumpCutoff { get; private set; }

        /// <summary>
        /// The amount of time after a fall where you can still initiate a jump.
        /// </summary>
        public static float CoyoteTime { get; private set; }

        /// <summary>
        /// The amount of the time player will auto jump after hitting the ground if you pressed jump early.
        /// </summary>
        public static float JumpBufferTime { get; private set; }

        /// <summary>
        /// Max fall speed.
        /// </summary>
        public static float TerminalVelocity { get; private set; }

        // Private class to deserialize JSON
        private class Settings
        {
            public float maxRunSpeed { get; set; }
            public float maxWalkSpeed { get; set; }
            public float runAcceleration { get; set; }
            public float runDeceleration { get; set; }
            public float turnSpeed { get; set; }
            public float jumpHeight { get; set; }
            public float earthGravity { get; set; }
            public float moonGravity { get; set; }
            public float waterGravity { get; set; }
            public float jumpDuration { get; set; }
            public float airAcceleration { get; set; }
            public float airControl { get; set; }
            public float airBreak { get; set; }
            public float jumpCutoff { get; set; }
            public float coyoteTime { get; set; }
            public float jumpBufferTime { get; set; }
            public float terminalVelocity { get; set; }
        }

        public static void Initialize()
        {
            // Get the path to the PlayerSettings.json file in the output directory (where the game runs)
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlayerSettings.json");

            // Get the path to the source file (in the project directory)
            // Go up from bin/Debug/net6.0 to the project root, then to the source file
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            _sourceFilePath = Path.Combine(projectRoot, "PlayerSettings.json");

            System.Diagnostics.Debug.WriteLine($"PlayerSettings output path: {_filePath}");
            System.Diagnostics.Debug.WriteLine($"PlayerSettings source path: {_sourceFilePath}");

            // Load initial settings from the output directory
            LoadSettings();

            // Set up file watcher for hot-reload on the SOURCE directory
            SetupFileWatcher();
        }

        private static void LoadSettings()
        {
            lock (_lockObject)
            {
                var json = File.ReadAllText(_filePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings != null)
                {
                    MaxRunSpeed = settings.maxRunSpeed;
                    MaxWalkSpeed = settings.maxWalkSpeed;
                    RunAcceleration = settings.runAcceleration;
                    RunDeceleration = settings.runDeceleration;
                    TurnSpeed = settings.turnSpeed;
                    JumpHeight = settings.jumpHeight;
                    EarthGravity = settings.earthGravity;
                    MoonGravity = settings.moonGravity;
                    WaterGravity = settings.waterGravity;
                    JumpDuration = settings.jumpDuration;
                    AirAcceleration = settings.airAcceleration;
                    AirControl = settings.airControl;
                    AirBreak = settings.airBreak;
                    JumpCutoff = settings.jumpCutoff;
                    CoyoteTime = settings.coyoteTime;
                    JumpBufferTime = settings.jumpBufferTime;
                    TerminalVelocity = settings.terminalVelocity;
                }
            }
        }

        private static void SetupFileWatcher()
        {
            try
            {
                // Watch the source file directory, not the output directory
                var directory = Path.GetDirectoryName(_sourceFilePath);
                var fileName = Path.GetFileName(_sourceFilePath);

                if (directory != null && fileName != null && Directory.Exists(directory))
                {
                    _watcher = new FileSystemWatcher(directory, fileName);
                    _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                    _watcher.Changed += OnFileChanged;
                    _watcher.EnableRaisingEvents = true;

                    System.Diagnostics.Debug.WriteLine($"File watcher set up for: {_sourceFilePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up file watcher: {ex.Message}");
            }
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce: prevent multiple rapid reloads
            var now = DateTime.Now;
            if ((now - _lastReloadTime).TotalMilliseconds < 500)
            {
                return;
            }

            _lastReloadTime = now;

            // Wait a moment for the file to be fully written
            System.Threading.Thread.Sleep(100);

            try
            {
                System.Diagnostics.Debug.WriteLine("PlayerSettings.json changed in source, copying to output and reloading...");

                // Copy the source file to the output directory
                File.Copy(_sourceFilePath, _filePath, overwrite: true);

                // Wait a bit for the copy to complete
                System.Threading.Thread.Sleep(50);

                // Reload the settings from the output directory
                LoadSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying and reloading PlayerSettings: {ex.Message}");
            }
        }

        public static void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
