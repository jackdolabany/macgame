using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using TileEngine;
using MacGame.Items;
using System.Linq;

namespace MacGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        public const string StartingWorld = "IntroLevel";
        private const bool startAtTitleScreen = false;
        public static bool IS_DEBUG = true;

        public const int TacosNeeded = 100;

        public readonly static Vector2 EarthGravity = new Vector2(0, 1600);
        public readonly static Vector2 MoonGravity = new Vector2(0, 700);
        public static Vector2 Gravity = new Vector2(0, 1600);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public const int GAME_X_RESOLUTION = 128 * TileScale;
        public const int GAME_Y_RESOLUTION = 128 * TileScale;

        public static Random Randy = new Random();
        public const float MIN_DRAW_INCREMENT = 0.000001f;

        public static bool DrawAllCollisisonRects = false;

        /// <summary>
        /// 8 x 8 Tiles
        /// </summary>
        public static Texture2D TileTextures;
        
        /// <summary>
        /// 16 x 16 tiles
        /// </summary>
        public static Texture2D BigTileTextures;

        /// <summary>
        /// 24 x 24 tiles
        /// </summary>
        public static Texture2D ReallyBigTileTextures;

        public static Texture2D titleScreen;

        public static Rectangle WhiteSourceRect;

        public const int TileSize = TileMap.TileSize;
        
        /// <summary>
        /// The scale that the content processor will apply to the tiles.
        /// For this game, we have 8x8 tiles, but they are scaled up to 32x32 for smooth scrolling.
        /// </summary>
        public const int TileScale = 4;

        private static RenderTarget2D gameRenderTarget;

        public static Player Player;

        private static SceneManager sceneManager;

        public static Level CurrentLevel;

        /// <summary>
        /// putting this here so it can easily be seen.
        /// </summary>
        public const string SaveGameFolder = "MacsAdventure";

        public const string HubWorld = "TestHub";
        public const string IntroLevel = "IntroLevel";

        // Set in the ctor
        public static int TotalSocks { get; private set; }

        AlertBoxMenu gotASockMenu;

        /// <summary>
        /// If you aren't in a hub world, this is the name of the door you came from.
        /// You'd return here if you quit or die.
        /// </summary>
        public static string HubDoorNameYouCameFrom = "";

        private static bool drawHappened = true;

        /// <summary>
        /// This represents the wave at the top of water. There's only one because we're using the flyweight pattern.
        /// </summary>
        public static WaterWaveFlyweight WaterWaveFlyweight;
        public static WaterWaveFlyweight WaterWaveFlyweightAlt;

        public enum TransitionType
        {
            /// <summary>
            /// State changes immediately with no transition effects.
            /// </summary>
            Instant,

            /// <summary>
            /// Screen fades to black, then the state changes, then the screen fades to clear.
            /// </summary>
            SlowFade,

            /// <summary>
            /// Same as fade, but much faster for quick level transitions.
            /// </summary>
            FastFade
        }

        public static IEnumerable<Platform> Platforms
        {
            get
            {
                return CurrentLevel.Platforms;
            }
        }

        public static TileMap CurrentMap
        {
            get
            {
                return CurrentLevel?.Map;
            }
        }

        public static Camera Camera;
        private static KeyboardState previousKeyState;

        public static SpriteFont Font;

        private PauseMenu pauseMenu;
        private MainMenu mainMenu;

        private GameState _gameState;

        /// <summary>
        /// How long to show the title screen after the intro.
        /// </summary>
        private float _titleScreenAfterIntroTimer = 0f;

        private GameState CurrentGameState
        {
            get { return _gameState; }
            set
            {
                _gameState = value;
                transitionToState = value;
            }
        }

        // State to go to on the next update cycle
        private GameState transitionToState;
        private TransitionType transitionType;
        private float transitionTimer;

        const float totalTransitionTime = 0.5f;

        // After loading assets MonoGame likes to skip  bunch of draw calls. So we'll
        // wait a little bit of time after loading to transition to the Playing state. 
        // Otherwise transitions will be janky.
        static float waitAfterLoadingTimer;

        private bool IsFading;

        // Map to go to, but first we need to transition to the loading screen and stuff
        private string _goToMap;
        private string _putPlayerAtDoor;

        InputManager inputManager;

        // Some number strings so that we don't need to create garbage by boxing and unboxing numbers.
        public static string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        /// <summary>
        /// The current game state that can be saved or loaded.
        /// </summary>
        public static StorageState StorageState { get; set; }

        /// <summary>
        /// State about the current level that will reset if you go to the hub or a new level from the hub.
        /// </summary>
        public static LevelState LevelState { get; set; }

        public static void ThrowDebugException(string message)
        {
            if (Game1.IS_DEBUG)
            {
                throw new Exception(message);
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            var scale = 2;

            graphics.PreferredBackBufferWidth = GAME_X_RESOLUTION * scale;
            graphics.PreferredBackBufferHeight = GAME_Y_RESOLUTION * scale;

            Window.AllowUserResizing = true;
            Window.Title = "Mac Game";

            Content.RootDirectory = "Content";

            GlobalEvents.SockCollected += OnSockCollected;
            GlobalEvents.DoorEntered += OnDoorEntered;
            GlobalEvents.BeginDoorEnter += OnBeginDoorEnter;
            GlobalEvents.IntroComplete += OnIntroComplete;

            var whiteTileRect = Helpers.GetTileRect(1, 3);

            // Grab a 2 x 2 rectangle from the middle of this tile rect.
            WhiteSourceRect = new Rectangle(whiteTileRect.X + 4, whiteTileRect.Y + 4, 2, 2);

            TotalSocks = SockIndex.LevelNumberToSocks.Values.SelectMany(c => c).Count();

            LevelState = new LevelState();

            // Validate the SockIndex
            foreach (var key in SockIndex.LevelNumberToSocks.Keys)
            {
                var socks = SockIndex.LevelNumberToSocks[key];
                var names = socks.Select(c => c.Name);

                // Make sure the names are unique.
                if (names.Distinct().Count() != names.Count())
                {
                    throw new Exception($"Sock names in world {key} are not unique.");
                }
            }
        }

        private void OnSockCollected(object? sender, EventArgs e)
        {
            pauseForSockTimer = 3f;
            SoundManager.PlaySound("SockCollected", 0.4f);
            TransitionToState(GameState.GotSock, TransitionType.Instant);
        }

        private void OnDoorEntered(object? sender, DoorEnteredEventArgs args)
        {
            // Set these and transition to loading which is a black screen. Then
            // that game state will actually load the level.
            _goToMap = args.TransitionToMap;
            _putPlayerAtDoor = args.PutPlayerAtDoor;

            if (Game1.CurrentLevel.IsHubWorld && !string.IsNullOrEmpty(args.DoorNameEntered))
            {
                Game1.HubDoorNameYouCameFrom = args.DoorNameEntered;
            }

            // Immediately pause
            TransitionToState(GameState.PausedForAction, TransitionType.Instant);

            // Then transition to loading.
            TransitionToState(GameState.LoadingLevel, TransitionType.FastFade);
        }

        private void OnBeginDoorEnter(object? sender, EventArgs args)
        {
            TransitionToState(GameState.PausedForAction, TransitionType.Instant);
        }

        /// <summary>
        /// The initial intro level is complete, save the game and send them to the hub.
        /// </summary>
        private void OnIntroComplete(object? sender, EventArgs args)
        {
            // How many seconds to re-show the title screen after the intro.
            // Might make this another screen later.
            _titleScreenAfterIntroTimer = 3f;
            StorageState.HasBeatenIntroLevel = true;
            StorageManager.TrySaveGame();
            TransitionToState(GameState.TitleFromIntro, TransitionType.SlowFade);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // SynchronizeWithVerticalRetrace syncs the draw calls with the monitor refresh rate
            // graphics.SynchronizeWithVerticalRetrace = true;

            // IsFixedTimeStep guarantees each frame is 1 60th of a call. Inserts an extra update call if needed.
            //this.IsFixedTimeStep = true;

            sceneManager = new SceneManager();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            TileTextures = Content.Load<Texture2D>(@"Textures\Textures");
            BigTileTextures = Content.Load<Texture2D>(@"Textures\BigTextures");
            ReallyBigTileTextures = Content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");

            Font = Content.Load<SpriteFont>(@"Fonts\KenPixel");
            //Font = Content.Load<SpriteFont>(@"Fonts\emulogic");
            inputManager = new InputManager();
            var deadMenu = new DeadMenu(this);

            // Flyweights
            WaterWaveFlyweight = new WaterWaveFlyweight(false);
            WaterWaveFlyweightAlt = new WaterWaveFlyweight(true);

            Game1.StorageState = new StorageState(1);
            Player = new Player(Content, inputManager, deadMenu);
            
            // test
            Player.WorldLocation = new Vector2(10, 10);

            gameRenderTarget = new RenderTarget2D(GraphicsDevice, GAME_X_RESOLUTION, GAME_Y_RESOLUTION, false, SurfaceFormat.Color, DepthFormat.None);

            // Basic Camera Setup
            Camera = new Camera();
            Camera.Zoom = Camera.DEFAULT_ZOOM;
            Camera.ViewPortWidth = Game1.GAME_X_RESOLUTION;
            Camera.ViewPortHeight = Game1.GAME_Y_RESOLUTION;

            SoundManager.Initialize(Content);
            StorageManager.Initialize(TileTextures, this);
            EffectsManager.Initialize(Content);

            pauseMenu = new PauseMenu(this);
            mainMenu = new MainMenu(this);

            if (startAtTitleScreen)
            {
                TransitionToState(GameState.TitleScreen, TransitionType.Instant);
            }
            else
            {
                if (StartingWorld == HubWorld)
                {
                    GoToHub(false);
                }
                else
                {
                    CurrentLevel = sceneManager.LoadLevel(StartingWorld, Content, Player, Camera);
                }

                Camera.Map = CurrentLevel.Map;
                TransitionToState(GameState.Playing, TransitionType.Instant);
            }

            ConversationManager.Initialize(Content);
            CutsceneManager.Initialize(Content);

            gotASockMenu = new AlertBoxMenu(this, "You got a Sock!", (a, b) =>
            {
                MenuManager.ClearMenus();

                StorageManager.TrySaveGame();
                TransitionToState(GameState.Playing, TransitionType.Instant);
            });
        }

        /// <param name="isYeet">True to yeet the player out of the door as punishment for quitting or dying.</param>
        public void GoToHub(bool isYeet)
        {
            MenuManager.ClearMenus();

            TransitionToState(GameState.Playing);

            pauseMenu.SetupTitle("Paused");

            string hubDoorPlayerCameFrom = "";
            if (CurrentLevel != null && !string.IsNullOrEmpty(Game1.HubDoorNameYouCameFrom))
            {
                hubDoorPlayerCameFrom = Game1.HubDoorNameYouCameFrom;
            }

            Player.ResetStateForLevelTransition(true);
            LevelState.Reset();

            CurrentLevel = sceneManager.LoadLevel(HubWorld, Content, Player, Camera);
            Camera.Map = CurrentLevel.Map;

            // Place the player at the door they came from.
            if (hubDoorPlayerCameFrom != "")
            {
                foreach (var door in CurrentLevel.Doors)
                {
                    if (door.Name == hubDoorPlayerCameFrom)
                    {
                        if (isYeet)
                        {
                            door.PlayerSlidingOut();
                        }
                        else
                        {
                            Player.WorldLocation = door.WorldLocation;
                        }
                        break;
                    }
                }
            }
        }

        public void StartNewGame()
        {
            MenuManager.ClearMenus();
            TransitionToState(GameState.Playing);
            pauseMenu.SetupTitle("Paused");
            Player.ResetStateForLevelTransition(true);
            LevelState.Reset();
            CurrentLevel = sceneManager.LoadLevel(IntroLevel, Content, Player, Camera);
            Camera.Map = CurrentLevel.Map;
        }

        public void RestartLevel()
        {
            Player.Health = Player.MaxHealth;
            MenuManager.ClearMenus();
            TransitionToState(GameState.Playing);
            CurrentLevel = sceneManager.LoadLevel(CurrentLevel.Name, Content, Player, Camera);
            Camera.Map = CurrentLevel.Map;
        }

        public void GoToTitleScreen()
        {
            MenuManager.ClearMenus();
            TransitionToState(GameState.TitleScreen);
        }

        public void Pause()
        {
            TransitionToState(GameState.PausedWithMenu, TransitionType.Instant);
            MenuManager.AddMenu(pauseMenu);
        }

        public void Unpause()
        {
            MenuManager.ClearMenus();            
            TransitionToState(GameState.Playing, TransitionType.Instant);
        }

        /// <summary>
        /// This methods fades to black, transitions to the given state, and then fades back in.
        /// </summary>
        public void TransitionToState(GameState transitionToState, TransitionType transitionType = TransitionType.SlowFade)
        {
            if (transitionType == TransitionType.Instant)
            {
                this.transitionToState = transitionToState;
                CurrentGameState = transitionToState;
                transitionTimer = 0;
                IsFading = false;
                return;
            }

            IsFading = transitionType == TransitionType.SlowFade || transitionType == TransitionType.FastFade;
            if (this.transitionToState != transitionToState)
            {
                this.transitionToState = transitionToState;
                this.transitionType = transitionType;
                if (IsFading)
                {
                    transitionTimer = totalTransitionTime;
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (gameRenderTarget != null)
            {
                gameRenderTarget.Dispose();
                gameRenderTarget = null;
            }
        }

        float pauseForSockTimer = 0f;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Loading a level at runtime takes a while and causes MonoGame to skip a bunch of Draw
            // calls to catch up or something. We don't really want that.
            if (!drawHappened && this.CurrentGameState == GameState.LoadingLevel)
            {
                return;
            }
            else
            {
                drawHappened = false;
            }

            inputManager.ReadInputs();

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SoundManager.Update(elapsed);
            StorageManager.Update(elapsed);

            if (StorageManager.IsSavingOrLoading)
            {
                return;
            }

            // Menu manager update might unpause the game, we don't want to re-pause it on the same update frame.
            var isPaused = CurrentGameState == GameState.PausedWithMenu;

            MenuManager.Update(elapsed);

            if (_gameState == GameState.Playing)
            {

                WaterWaveFlyweight.Update(gameTime, elapsed);
                WaterWaveFlyweightAlt.Update(gameTime, elapsed);

                StorageState.TotalElapsedTime += elapsed;

                if (Player.Enabled && inputManager.CurrentAction.pause && !inputManager.PreviousAction.pause && !isPaused)
                {
                    Pause();
                }
                else if (ConversationManager.IsInConversation() && transitionTimer <= 0)
                {
                    CurrentGameState = GameState.Conversation;
                    return;
                }
                else
                {
                    CurrentLevel.Update(gameTime, elapsed);
                    EffectsManager.Update(gameTime, elapsed);
                    TimerManager.Update(elapsed);
                }
            }
            else if (_gameState == GameState.GotSock)
            {
                if (pauseForSockTimer > 0)
                {
                    pauseForSockTimer -= elapsed;
                    if (pauseForSockTimer <= 0)
                    {
                        MenuManager.AddMenu(gotASockMenu);
                    }
                }
            }
            else if (_gameState == GameState.LoadingLevel)
            {

                if (waitAfterLoadingTimer > 0)
                {
                    waitAfterLoadingTimer -= elapsed;
                }

                if (_goToMap != "" && CurrentLevel.Name != _goToMap)
                {
                    waitAfterLoadingTimer = 0.1f;

                    // TODO: Temp delete test code!!! bad
                    //System.Threading.Thread.Sleep(500);

                    var wasInHubWorld = CurrentLevel.IsHubWorld;

                    // This state will just draw a black screen for a frame until we can play.
                    CurrentLevel = sceneManager.LoadLevel(_goToMap, Content, Player, Camera);
                    Camera.Map = CurrentLevel.Map;

                    if (wasInHubWorld && !CurrentLevel.IsHubWorld)
                    {
                        pauseMenu.SetupTitle($"{CurrentLevel.Description}");
                    }
                    _goToMap = "";
                }

                // Player just went through a door, put him where he's supposed to be.
                if (!string.IsNullOrEmpty(_putPlayerAtDoor))
                {
                    foreach (var door in CurrentLevel.Doors)
                    {
                        if (door.Name == _putPlayerAtDoor)
                        {
                            Player.WorldLocation = door.WorldLocation;
                            break;
                        }
                    }
                    _putPlayerAtDoor = "";
                }

                // Wait a little bit of time after the level is loaded before transitioning. This is because MonoGame will
                // skip a bunch of draw calls to catch up after loading a level. This will make the transition look janky.
                if (waitAfterLoadingTimer <= 0)
                {
                    if (transitionToState != GameState.Playing)
                    {
                        // We must transition to playing but also immediately set the mode to Playing to 
                        // trick the stupid thing into thinking we just transitioned to Playing and we need to fade in
                        // from black.
                        TransitionToState(GameState.Playing, TransitionType.FastFade);
                        this.CurrentGameState = GameState.Playing;
                    }
                }
            }
            else if (_gameState == GameState.TitleScreen && transitionToState == GameState.TitleScreen)
            {
                if (!MenuManager.IsMenu)
                {
                    // Show the title screen menu only after we've transitioned here.
                    MenuManager.AddMenu(mainMenu);
                }
            }
            else if (_gameState == GameState.TitleFromIntro && transitionToState == GameState.TitleFromIntro)
            {
                // Wait until the timer is up and then go to the hub world.
                if (_titleScreenAfterIntroTimer > 0)
                {
                    _titleScreenAfterIntroTimer -= elapsed;
                }
                else
                {
                    GoToHub(false);
                }
            }
            else if (CurrentGameState == GameState.Conversation)
            {
                ConversationManager.Update(elapsed);
                CutsceneManager.Update(gameTime, elapsed);

                // If the conversation is over, and they aren't transitioning into another state, go back to playing.
                if (!ConversationManager.IsInConversation() && transitionToState == GameState.Conversation)
                {
                    CurrentGameState = GameState.Playing;
                }
            }
            else if (CurrentGameState == GameState.PausedForAction)
            {
                // Limited update of things that may transition state.
                // This shouldn't update the player or enemies.
                TimerManager.Update(elapsed);
                CurrentLevel.PausedUpdate(gameTime, elapsed);
            }
            //else if (CurrentGameState == GameState.PausedWithMenu)
            //{
            //    // Check if they are trying to unpause.
            //    if (inputManager.CurrentAction.pause && !inputManager.PreviousAction.pause)
            //    {
            //        Unpause();
            //    }
            //}

            if (Game1.IS_DEBUG)
            {
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.I))
                {
                    Camera.Zoom += 0.4f * elapsed;
                }
                else if (keyState.IsKeyDown(Keys.O))
                {
                    Camera.Zoom -= 0.4f * elapsed;
                }
                else if (keyState.IsKeyDown(Keys.R))
                {
                    Camera.Zoom = Camera.DEFAULT_ZOOM;
                }

                if (keyState.IsKeyDown(Keys.D) && !previousKeyState.IsKeyDown(Keys.D))
                {
                    Game1.DrawAllCollisisonRects = !Game1.DrawAllCollisisonRects;
                }
                previousKeyState = keyState;
            }

            // Handle transitions
            var multiplier = this.transitionType == TransitionType.FastFade ? 2.5f : 1f;
            if (transitionTimer > 0)
            {
                transitionTimer -= (elapsed * multiplier);
            }

            if (transitionTimer <= 0 && transitionToState != CurrentGameState)
            {
                // set the timer to transition/fade back in
                transitionTimer = totalTransitionTime;
                CurrentGameState = transitionToState;
            }

            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            drawHappened = true;

            Camera.UpdateTransformation();
            var cameraTransformation = Camera.Transform;

            // We'll draw everything to gameRenderTarget, including the white render target.
            GraphicsDevice.SetRenderTarget(gameRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            switch (_gameState)
            {
                case GameState.Playing:
                case GameState.PausedWithMenu:
                case GameState.GotSock:
                case GameState.Conversation:
                case GameState.PausedForAction:

                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        cameraTransformation);

                    CurrentLevel.Draw(spriteBatch, Camera.ViewPort);

                    CutsceneManager.Draw(spriteBatch);

                    EffectsManager.Draw(spriteBatch);

                    spriteBatch.End();

                    // Draw the HUD over everything.
                    spriteBatch.Begin();
                    DrawHud(spriteBatch);

                    if (CurrentGameState == GameState.Conversation)
                    {
                        ConversationManager.Draw(spriteBatch);
                    }

                    spriteBatch.End();

                    //// Test draw the processed textures
                    //spriteBatch.Begin(SpriteSortMode.Deferred,
                    // BlendState.AlphaBlend,
                    // SamplerState.PointClamp,
                    // null,
                    // null,
                    // null,
                    // cameraTransformation);
                    //spriteBatch.Draw(TileTextures, new Rectangle(0, 0, 1000, 1000), WhiteSourceRect, Color.Black);
                    //spriteBatch.Draw(TileTextures, new Vector2(0, 0), Color.White);
                    //spriteBatch.End();

                    break;
                
                case GameState.LoadingLevel:
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        cameraTransformation);

                    spriteBatch.Draw(TileTextures, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), WhiteSourceRect, Color.Black);

                    spriteBatch.End();
                    break;
               
                case GameState.TitleScreen:
                case GameState.TitleFromIntro:

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    spriteBatch.Draw(titleScreen, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Color.White);
                    spriteBatch.DrawString(Font, "Mac's\nRidiculous\nAdventure", new Vector2(Game1.TileSize, Game1.TileSize), Color.DarkGray, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(Font, "Mac's\nRidiculous\nAdventure", new Vector2(Game1.TileSize + 4, Game1.TileSize - 4), Color.Black, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                    spriteBatch.End();
                    break;
                default:
                    throw new NotImplementedException($"Invalid game state: {_gameState}");
            }

            // Draw the menus to a new sprite batch ignoring the camera stuff.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
            MenuManager.Draw(spriteBatch);

            // Draw the saving/loading menu
            StorageManager.Draw(spriteBatch);

            // Draw some fading black over the screen if we are transitioning between screens
            if (transitionTimer > 0 && IsFading)
            {
                float opacity = (transitionTimer / totalTransitionTime);
                // fading in vs fading out.
                // 0 is transparent, 1 is black.
                if (CurrentGameState != transitionToState)
                {
                    // Fade towards black.
                    opacity = 1.0f - opacity;
                }

                DrawBlackOverScreen(spriteBatch, opacity);
            }

            spriteBatch.End();

            // Switch back to drawing onto the back buffer. This is the default space in memory, the size is determined by the ClientWindow. 
            // When the present call is made, the backbuffer will show up as the new screen.
            GraphicsDevice.SetRenderTarget(null);

            // XNA draws a bright purple color to the backbuffer by default when we switch to it. Lame! Let's clear it out.
            GraphicsDevice.Clear(Color.Black);

            // Draw the gameRenderTarget with everything in it to the back buffer. We'll reuse spritebatch and just stretch it to fit.
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // We need to stretch the image to fit the screen size. 
            spriteBatch.Draw(gameRenderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static void DrawHud(SpriteBatch spriteBatch)
        {
            // Draw the hearts in the HUD
            var hudYPos = 3;
            for (int i = 0; i < Player.MaxHealth; i++)
            {
                var heartXPos = 8 + (i * TileSize);
                if (i < Player.Health)
                {
                    spriteBatch.Draw(TileTextures, new Rectangle(heartXPos, hudYPos, TileSize, TileSize), Helpers.GetTileRect(1, 2), Color.White);
                }
                else
                {
                    spriteBatch.Draw(TileTextures, new Rectangle(heartXPos, hudYPos, TileSize, TileSize), Helpers.GetTileRect(2, 2), Color.White);
                }
            }

            // Draw the player's current item
            if (Player.CurrentItem != null)
            {
                spriteBatch.Draw(TileTextures, new Rectangle(8, TileSize + 20, TileSize, TileSize), Player.CurrentItem.ItemIcon.Source, Color.White);
            }

            // Draw the number of tacos in the HUD for regular levels, or draw the socks for the Hub level.
            Rectangle imageSource;
            int count;

            if (CurrentLevel.IsHubWorld)
            {
                var SockSourceRect = Helpers.GetTileRect(9, 2);
                DrawNumberOfThingsOnRight(spriteBatch, SockSourceRect, Player.SockCount, GAME_X_RESOLUTION - 40, hudYPos);
            }
            else if (CurrentLevel.LevelNumber > 0) // Don't show tacos in the hub or intro levels.
            {
                var tacoIconSource = Helpers.GetTileRect(8, 2);
                DrawNumberOfThingsOnRight(spriteBatch, tacoIconSource, Player.Tacos, GAME_X_RESOLUTION - 40, hudYPos);
            }

            // Draw red/green/blue keys on the right below the tacos
            Vector2 keyLocation = new Vector2(GAME_X_RESOLUTION - 40, hudYPos + 48);
            var level = Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber];
            if (level.Keys.HasRedKey)
            {
                var redKeySourceRect = Helpers.GetTileRect(13, 4);
                spriteBatch.Draw(TileTextures, keyLocation, redKeySourceRect, Color.White);
                keyLocation.Y += 24;
            }
            if (level.Keys.HasGreenKey)
            {
                var greenKeySourceRect = Helpers.GetTileRect(14, 4);
                spriteBatch.Draw(TileTextures, keyLocation, greenKeySourceRect, Color.White);
                keyLocation.Y += 24;
            }
            if (level.Keys.HasBlueKey)
            {
                var blueKeySourceRect = Helpers.GetTileRect(15, 4);
                spriteBatch.Draw(TileTextures, keyLocation, blueKeySourceRect, Color.White);
            }
        }

        /// <summary>
        /// Draws a thing and a count of it to the right side of the screen.
        /// </summary>
        private static void DrawNumberOfThingsOnRight(SpriteBatch spriteBatch, Rectangle iconSourceRectangle, int count, int rightMostX, int yPos)
        {
            int onesPlace = count % 10;

            spriteBatch.DrawString(Font, Numbers[onesPlace], new Vector2(rightMostX, yPos), Color.White);

            if (count > 9)
            {
                int tensPlace = (count / 10) % 10;
                rightMostX -= Game1.TileSize;
                spriteBatch.DrawString(Font, Numbers[tensPlace], new Vector2(rightMostX, yPos), Color.White);
            }
            if (count > 99)
            {
                int hundredsPlace = (count / 100) % 10;
                rightMostX -= Game1.TileSize;
                spriteBatch.DrawString(Font, Numbers[hundredsPlace], new Vector2(rightMostX, yPos), Color.White);
            }

            // Draw the icon image
            spriteBatch.Draw(TileTextures, new Rectangle(rightMostX - Game1.TileSize - 4, yPos + 8, Game1.TileSize, Game1.TileSize), iconSourceRectangle, Color.White);
        }

        public static void DrawBlackOverScreen(SpriteBatch spriteBatch, float opacity)
        {
            spriteBatch.Draw(Game1.TileTextures, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Game1.WhiteSourceRect, Color.Black * opacity);
        }

        public void LoadSavedGame(StorageState? ss, int saveSlot)
        {

            if (ss == null)
            {
                ss = new StorageState(saveSlot);
            }

            SoundManager.StopSong();
            MenuManager.ClearMenus();
            Game1.StorageState = (StorageState)ss.Clone();

            Player.SockCount = Game1.StorageState.Levels.Select(l => l.Value).Sum(l => l.CollectedSocks.Count);

            _goToMap = "";
            _putPlayerAtDoor = "";
            LevelState.Reset();

            CurrentLevel = sceneManager.LoadLevel(HubWorld, Content, Player, Camera);
            Camera.Map = CurrentLevel.Map;

            if (ss.HasBeatenIntroLevel)
            {
                GoToHub(false);
            }
            else
            {
                StartNewGame();
            }
        }

        public static void TacoCollected(string levelName, int x, int y)
        {
            if (!LevelState.MapNameToCollectedTacos.ContainsKey(levelName))
            {
                LevelState.MapNameToCollectedTacos[levelName] = new List<Vector2>();
            }

            LevelState.MapNameToCollectedTacos[levelName].Add(new Vector2(x, y));
        }

        public static bool WasTacoCollected(string levelName, int x, int y)
        {
            if (LevelState.MapNameToCollectedTacos.ContainsKey(levelName))
            {
                var list = LevelState.MapNameToCollectedTacos[levelName];
                return list.Contains(new Vector2(x, y));
            }
            return false;
        }

        public enum GameState
        {
            TitleScreen,

            Playing,
            
            /// <summary>
            /// After the intro level we'll fade to the title screen and then again to the hub level.
            /// </summary>
            TitleFromIntro,

            /// <summary>
            /// Freeze the game for a moment but still draw and play a jingle when you get a sock.
            /// </summary>
            GotSock,
            
            /// <summary>
            /// Gameplay with some menu displaying. The menu will transition the state back.
            /// </summary>
            PausedWithMenu,
            
            /// <summary>
            /// Talking to an NPC or text coming up because of some action (trying to open a locked door, etc.)
            /// </summary>
            Conversation,

            /// <summary>
            /// Use this to pause the game for some kind of action, like a door opening and closing.
            /// The enemies and player won't update but something else will trigger a later state transition.
            /// </summary>
            PausedForAction,

            LoadingLevel,

            Dead
        }
    }
}
