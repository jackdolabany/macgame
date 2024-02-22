using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using TileEngine;
using System.Globalization;

namespace MacGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public const int GAME_X_RESOLUTION = 128;
        public const int GAME_Y_RESOLUTION = 128;

        public static Random Randy = new Random();
        public const float MIN_DRAW_INCREMENT = 0.0000005f;

        public static bool DrawAllCollisisonRects = false;

        public static Texture2D textures;
        public static Texture2D titleScreen;

        public static Rectangle whiteSourceRect = new Rectangle(11, 27, 2, 2);

        public const int TileSize = TileEngine.TileMap.TileSize;

        private static RenderTarget2D gameRenderTarget;

        public static Player Player;

        private static SceneManager sceneManager;

        private static Level currentLevel;

        public static string TransitionToMap;
        public static string PutPlayerAtDoor;

        public static IEnumerable<Platform> Platforms
        {
            get
            {
                return currentLevel.Platforms;
            }
        }

        public static TileMap CurrentMap
        {
            get
            {
                return currentLevel?.Map;
            }
        }

        public static bool IS_DEBUG = true;

        public static Camera Camera;
        private static KeyboardState previousKeyState;

        public static SpriteFont Font;

        private PauseMenu pauseMenu;
        private MainMenu mainMenu;

        private GameState _gameState;

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
        private float transitionTimer;
        const float totalTransitionTime = 0.5f;
        private bool IsFading;

        InputManager inputManager;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            var scale = 6;

            graphics.PreferredBackBufferWidth = GAME_X_RESOLUTION * scale;
            graphics.PreferredBackBufferHeight = GAME_Y_RESOLUTION * scale;

            Window.AllowUserResizing = true;
            Window.Title = "Mac Game";

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
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

            textures = Content.Load<Texture2D>(@"Textures\Textures");
            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");

            var kenPixelFont = Content.Load<SpriteFont>(@"Fonts\KenPixel");
            Font = kenPixelFont;
            inputManager = new InputManager();
            var deadMenu = new DeadMenu(this);

            Player = new Player(Content, inputManager, deadMenu);

            // test
            Player.WorldLocation = new Vector2(10, 10);

            gameRenderTarget = new RenderTarget2D(GraphicsDevice, GAME_X_RESOLUTION, GAME_Y_RESOLUTION, false, SurfaceFormat.Color, DepthFormat.None);

            Camera = new Camera();

            SoundManager.Initialize(Content);

            // Load map and adjust Camera
            currentLevel = sceneManager.LoadLevel("TestLevel2", Content, Player, Camera);

            Camera.Map = currentLevel.Map;

            // Basic Camera Setup
            Camera.Zoom = Camera.DEFAULT_ZOOM;
            Camera.ViewPortWidth = Game1.GAME_X_RESOLUTION;
            Camera.ViewPortHeight = Game1.GAME_Y_RESOLUTION;

            EffectsManager.Initialize(Content);
            
            pauseMenu = new PauseMenu(this);
            mainMenu = new MainMenu(this);

            CurrentGameState = GameState.Playing;

            StartNewGame(false);
        }

        public void StartNewGame(bool loadLevel = true)
        {
            MenuManager.ClearMenus();

            TransitionToState(GameState.Playing);

            EffectsManager.Initialize(Content);

            Player.Enabled = true;
            Player.Health = Player.MaxHealth;

            if (loadLevel)
            {
                currentLevel = sceneManager.LoadLevel("TestLevel2", Content, Player, Camera);
                Camera.Map = currentLevel.Map;
            }
        }

        public void GoToTitleScreen()
        {
            MenuManager.ClearMenus();
            TransitionToState(GameState.TitleScreen);

        }

        public void Pause()
        {
            TransitionToState(GameState.Paused, false);
            MenuManager.AddMenu(pauseMenu);
        }

        public void Unpause()
        {
            MenuManager.RemoveTopMenu();
            TransitionToState(GameState.Playing, false);
        }

        public void TransitionToState(GameState transitionToState, bool isFading = true)
        {
            IsFading = isFading;
            if (this.transitionToState != transitionToState)
            {
                this.transitionToState = transitionToState;
                if (IsFading)
                {
                    transitionTimer = totalTransitionTime;
                }
            }
        }

        public bool IsTransitioningOut()
        {
            return transitionTimer > 0 && CurrentGameState != transitionToState;
        }

        public bool IsTransitioningOut(GameState expectedGameState)
        {
            return CurrentGameState != expectedGameState || IsTransitioningOut();
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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            inputManager.ReadInputs();

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MenuManager.Update(elapsed);
            SoundManager.Update(elapsed);

            if (_gameState == GameState.Playing)
            {
                if (Player.Enabled && inputManager.CurrentAction.pause && !inputManager.PreviousAction.pause)
                {
                    Pause();
                }
                else
                {
                    currentLevel.Update(gameTime, elapsed);
                    EffectsManager.Update(gameTime, elapsed);
                    TimerManager.Update(elapsed);
                }

                // See if it's time to go to another level.
                if (!string.IsNullOrEmpty(TransitionToMap))
                {
                    currentLevel = sceneManager.LoadLevel(TransitionToMap, Content, Player, Camera);
                    Camera.Map = currentLevel.Map;
                    TransitionToMap = null;

                    if (!string.IsNullOrEmpty(PutPlayerAtDoor))
                    {
                        foreach(var door in currentLevel.Doors)
                        {
                            if (door.Name == PutPlayerAtDoor)
                            {
                                Player.WorldLocation = door.WorldLocation;
                                break;
                            }
                        }
                        
                        PutPlayerAtDoor = null;

                    }
                }
            }

            if (_gameState == GameState.TitleScreen && transitionToState == GameState.TitleScreen)
            {
                if (!MenuManager.IsMenu)
                {
                    // Show the title screen menu only after we've transitioned here.
                    MenuManager.AddMenu(mainMenu);
                }
            }

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
            if (transitionTimer > 0)
            {
                transitionTimer -= elapsed;
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
            Camera.UpdateTransformation(GraphicsDevice);
            var cameraTransformation = Camera.Transform;

            // We'll draw everything to gameRenderTarget, including the white render target.
            GraphicsDevice.SetRenderTarget(gameRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            switch (_gameState)
            {
                case GameState.Playing:
                case GameState.Paused:

                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        cameraTransformation);

                    currentLevel.Draw(spriteBatch, Camera.ViewPort);

                    EffectsManager.Draw(spriteBatch);

                    // Draw the HUD
                    for (int i = 0; i < Player.Health; i++)
                    {
                        var xPos = (int)Camera.ViewPort.X + 2 + (i * 8);
                        spriteBatch.Draw(textures, new Rectangle(xPos, 2, 8, 8), new Rectangle(8, 16, 8, 8), Color.White);
                    }
                    break;
                case GameState.TitleScreen:

                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
                    spriteBatch.Draw(titleScreen, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Color.White);

                    break;
                default:
                    throw new NotImplementedException($"Invalid game state: {_gameState}");
            }

            spriteBatch.End();

            // Draw the menus to a new sprite batch ignoring the camera stuff.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
            MenuManager.Draw(spriteBatch);

            // Draw some fading black over the screen if we are transitioning between screens
            if (transitionTimer > 0 && IsFading)
            {
                float opacity = (transitionTimer / totalTransitionTime);
                // fading in vs fading out.
                if (CurrentGameState != transitionToState)
                {
                    opacity = 1.0f - opacity;
                }
                DrawBlackOverScreen(spriteBatch, opacity);
            }

            spriteBatch.End();

            // Switch back to drawing onto the back buffer. This is the default space in memory, the size is determined by the ClientWindow. 
            // When the present call is made, the backbuffer will show up as the new screen.
            GraphicsDevice.SetRenderTarget(null);

            // XNA draws a bright purple color to the backbuffer by default when we switch to it. Lame! Let's clear it out.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the gameRenderTarget with everything in it to the back buffer. We'll reuse spritebatch and just stretch it to fit.
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // We need to stretch the image to fit the screen size. 
            spriteBatch.Draw(gameRenderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static void DrawBlackOverScreen(SpriteBatch spriteBatch, float opacity)
        {
            spriteBatch.Draw(Game1.textures, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Game1.whiteSourceRect, Color.Black * opacity);
        }

        public enum GameState
        {
            TitleScreen,
            Playing,
            Paused,
            Dead
        }
    }
}
