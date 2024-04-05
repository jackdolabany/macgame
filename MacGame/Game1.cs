using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using TileEngine;
using System.Globalization;
using System.Runtime.CompilerServices;
using MacGame.Items;

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

        public static Texture2D Textures;
        public static Texture2D titleScreen;

        public static Rectangle WhiteSourceRect = new Rectangle(11, 27, 2, 2);

        public const int TileSize = TileEngine.TileMap.TileSize;

        private static RenderTarget2D gameRenderTarget;

        public static Player Player;

        private static SceneManager sceneManager;

        public static Level CurrentLevel;

        public const string StartingHubWorld = "TestHub";

        AlertBoxMenu gotACricketCoinMenu;

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

        // Some number strings so that we don't need to create garbage by boxing and unboxing numbers.
        public static string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            var scale = 8;

            graphics.PreferredBackBufferWidth = GAME_X_RESOLUTION * scale;
            graphics.PreferredBackBufferHeight = GAME_Y_RESOLUTION * scale;

            Window.AllowUserResizing = true;
            Window.Title = "Mac Game";

            Content.RootDirectory = "Content";

            GlobalEvents.CricketCoinCollected += OnCricketCoinCollected;
            GlobalEvents.DoorEntered += OnDoorEntered;
            GlobalEvents.SubWorldDoorEntered += OnSubWorldDoorEntered;
            GlobalEvents.OneHundredTacosCollected += OnOneHundredTacosCollected;
        }

        private void OnCricketCoinCollected(object? sender, EventArgs e)
        {
            pauseForCoinTimer = 2f;
            // TODO: play some kind of jingle.
            SoundManager.PlaySound("health");
            TransitionToState(GameState.GotCoin, false);
        }

        private void OnDoorEntered(object? sender, DoorEnteredEventArgs args)
        {

            CurrentLevel = sceneManager.LoadLevel(args.TransitionToMap, Content, Player, Camera);
            Camera.Map = CurrentLevel.Map;

            if(args.NewHintIndex.HasValue)
            {
                CurrentLevel.SelectedHintIndex = args.NewHintIndex.Value;

                // Only when a hint is selected do we care to also track the door you came from. This is so we can
                // boot you out the same door if you die.
                CurrentLevel.HubDoorNameYouCameFrom = args.DoorNameEntered;
            }

            var hint = "";
            if(CurrentLevel.CoinHints.ContainsKey(CurrentLevel.SelectedHintIndex))
            {
                hint = "\"" + CurrentLevel.CoinHints[CurrentLevel.SelectedHintIndex] + "\"";
            }

            pauseMenu.SetupTitle($"{CurrentLevel.Description}\n{hint}");

            // Player just went through a door, put him where he's supposed to be.
            if (!string.IsNullOrEmpty(args.PutPlayerAtDoor))
            {
                foreach (var door in CurrentLevel.Doors)
                {
                    if (door.Name == args.PutPlayerAtDoor)
                    {
                        Player.WorldLocation = door.WorldLocation;
                        break;
                    }
                }
            }
        }

        private void OnSubWorldDoorEntered(object? sender, SubWorldDoorEnteredEventArgs args)
        {
            var nextLevelInfo = sceneManager.GetNextLevelInfo(args.TransitionToMap, Content);

            // Show the hint menu and they'll get a chance to choose which level to go to.
            var hintMenu = new HintMenu(this, nextLevelInfo, args.DoorNameEntered); // Creating garbage, I know it's bad!
            MenuManager.AddMenu(hintMenu);
       
            TransitionToState(GameState.PausedWithMenu, false);
        }

        private void OnOneHundredTacosCollected(object? sender, EventArgs args)
        {
            // TODO: Some kind of reveal jingle
            SoundManager.PlaySound("health");

            var taco = (Taco)sender!;
            var tacoCoin = CurrentLevel.TacoCoin;

            // Shift it slightly because coins are larger than tacos.
            tacoCoin.WorldLocation = taco.WorldLocation;
            tacoCoin.Enabled = true;

            tacoCoinRevealTimer = 2f;

            // Block immmediate collection. The RevealTacoCoin state change will enable it again.
            tacoCoin.CanBeCollected = false;

            // Coin should bounce up then down.
            TimerManager.AddNewTimer(0.3f, () => tacoCoin.Velocity = new Vector2(0, -40f), false) // wait a sec and then move the coin up.
                .Then(0.5f, () => tacoCoin.Velocity = -tacoCoin.Velocity, false) // then down
                .Then(0.5f, () => tacoCoin.Velocity = Vector2.Zero, false); // then stop

            TransitionToState(GameState.RevealTacoCoin, false);
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

            Textures = Content.Load<Texture2D>(@"Textures\Textures");
            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");

            Font = Content.Load<SpriteFont>(@"Fonts\KenPixel");
            inputManager = new InputManager();
            var deadMenu = new DeadMenu(this);

            Player = new Player(Content, inputManager, deadMenu);

            // test
            Player.WorldLocation = new Vector2(10, 10);

            gameRenderTarget = new RenderTarget2D(GraphicsDevice, GAME_X_RESOLUTION, GAME_Y_RESOLUTION, false, SurfaceFormat.Color, DepthFormat.None);

            Camera = new Camera();

            SoundManager.Initialize(Content);

            // Load map and adjust Camera
            CurrentLevel = sceneManager.LoadLevel(StartingHubWorld, Content, Player, Camera);

            Camera.Map = CurrentLevel.Map;

            // Basic Camera Setup
            Camera.Zoom = Camera.DEFAULT_ZOOM;
            Camera.ViewPortWidth = Game1.GAME_X_RESOLUTION;
            Camera.ViewPortHeight = Game1.GAME_Y_RESOLUTION;

            EffectsManager.Initialize(Content);
            ConversationManager.Initialize(Content);

            pauseMenu = new PauseMenu(this);
            mainMenu = new MainMenu(this);

            CurrentGameState = GameState.Playing;

            gotACricketCoinMenu = new AlertBoxMenu(this, "You got a Cricket Coin!", (a, b) =>
            {
                TransitionToState(GameState.Playing);
                GoToHub(true, false);
            });

            GoToHub(false, false);
        }

        /// <param name="isReturning">True if the player died or selected to go back to the hub in the menu. False for the 
        /// start of a new game.</param>
        /// <param name="isYeet">True to yeet the player out of the door as punishment for quitting or dying.</param>
        public void GoToHub(bool isReturning, bool isYeet)
        {
            MenuManager.ClearMenus();

            TransitionToState(GameState.Playing);

            EffectsManager.Initialize(Content);

            pauseMenu.SetupTitle("Paused");

            Player.Enabled = true;
            Player.Health = Player.MaxHealth;
            Player.CurrentItem = null;
            Player.Tacos = 0;

            if (isReturning)
            {
                string hubDoorPlayerCameFrom = "";
                if (CurrentLevel != null && !string.IsNullOrEmpty(CurrentLevel.HubDoorNameYouCameFrom))
                {
                    hubDoorPlayerCameFrom = CurrentLevel.HubDoorNameYouCameFrom;
                }

                CurrentLevel = sceneManager.LoadLevel(StartingHubWorld, Content, Player, Camera);
                Camera.Map = CurrentLevel.Map;

                // Place the player at the door they came from.
                if (hubDoorPlayerCameFrom != "")
                {
                    foreach (var door in CurrentLevel.Doors)
                    {
                        if (door.Name == hubDoorPlayerCameFrom)
                        {
                            if(isYeet)
                            {
                                Player.SlideOutOfDoor(door.WorldLocation);
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
        }

        public void GoToTitleScreen()
        {
            MenuManager.ClearMenus();
            TransitionToState(GameState.TitleScreen);
        }

        public void Pause()
        {
            TransitionToState(GameState.PausedWithMenu, false);
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

        float pauseForCoinTimer = 0f;
        float tacoCoinRevealTimer = 0f;

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
            else if (_gameState == GameState.RevealTacoCoin)
            {
                // Pause for a bit while we play a jingle and just update the taco coin.
                TimerManager.Update(elapsed);
                CurrentLevel.TacoCoin.Update(gameTime, elapsed);
                tacoCoinRevealTimer -= elapsed;
                if(tacoCoinRevealTimer <= 0)
                {
                    // Replace the hard block on collection with a timer that will 
                    // make it flash for a bit.
                    CurrentLevel.TacoCoin.CanBeCollected = true;
                    CurrentLevel.TacoCoin.CanNotBeCollectedForTimer = 3f;
                    TransitionToState(GameState.Playing, false);
                }   
            }
            else if(_gameState == GameState.GotCoin)
            {
                if (pauseForCoinTimer > 0)
                {
                    pauseForCoinTimer -= elapsed;
                    if (pauseForCoinTimer <= 0)
                    {
                        MenuManager.AddMenu(gotACricketCoinMenu);
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
            else if (CurrentGameState == GameState.Conversation)
            {
                ConversationManager.Update(elapsed);
                if (!ConversationManager.IsInConversation())
                {
                    CurrentGameState = GameState.Playing;
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
                case GameState.PausedWithMenu:
                case GameState.GotCoin:
                case GameState.RevealTacoCoin:
                case GameState.Conversation:

                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        cameraTransformation);

                    CurrentLevel.Draw(spriteBatch, Camera.ViewPort);

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

                    break;
                case GameState.TitleScreen:

                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
                    spriteBatch.Draw(titleScreen, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Color.White);
                    spriteBatch.End();
                    break;
                default:
                    throw new NotImplementedException($"Invalid game state: {_gameState}");
            }

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

        public static void DrawHud(SpriteBatch spriteBatch)
        {
            // Draw the hearts in the HUD
            var hudYPos = 3;
            for (int i = 0; i < Player.MaxHealth; i++)
            {
                var heartXPos = 2 + (i * 8);
                if (i < Player.Health)
                {
                    spriteBatch.Draw(Textures, new Rectangle(heartXPos, hudYPos, 8, 8), new Rectangle(8, 16, 8, 8), Color.White);
                }
                else
                {
                    spriteBatch.Draw(Textures, new Rectangle(heartXPos, hudYPos, 8, 8), new Rectangle(16, 16, 8, 8), Color.White);
                }
            }

            // Draw the player's current item
            if (Player.CurrentItem != null)
            {
                spriteBatch.Draw(Textures, new Rectangle(2, 12, 8, 8), Player.CurrentItem.ItemIcon.Source, Color.White);
            }

            // Draw the number of tacos in the HUD for regular levels, or draw the Cricket coins for the Hub level.
            Rectangle imageSource;
            int count;

            if (CurrentLevel.IsHubWorld)
            {
                var cricketCoinSourceRect = new Rectangle(9 * TileSize, 2 * TileSize, 8, 8);
                DrawNumberOfThingsOnRight(spriteBatch, cricketCoinSourceRect, Player.CricketCoinCount, GAME_X_RESOLUTION - 10, hudYPos);
            }
            else
            {
                var tacoIconSource = new Rectangle(8 * TileSize, 2 * TileSize, 8, 8);
                DrawNumberOfThingsOnRight(spriteBatch, tacoIconSource, Player.Tacos, GAME_X_RESOLUTION - 10, hudYPos);
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
                rightMostX -= 8;
                spriteBatch.DrawString(Font, Numbers[tensPlace], new Vector2(rightMostX, yPos), Color.White);
            }
            if (count > 99)
            {
                int hundredsPlace = (count / 100) % 10;
                rightMostX -= 8;
                spriteBatch.DrawString(Font, Numbers[hundredsPlace], new Vector2(rightMostX, yPos), Color.White);
            }

            // Draw the icon image
            spriteBatch.Draw(Textures, new Rectangle(rightMostX - 8, yPos + 1, 8, 8), iconSourceRectangle, Color.White);
        }

        public static void DrawBlackOverScreen(SpriteBatch spriteBatch, float opacity)
        {
            spriteBatch.Draw(Game1.Textures, new Rectangle(0, 0, GAME_X_RESOLUTION, GAME_Y_RESOLUTION), Game1.WhiteSourceRect, Color.Black * opacity);
        }

        public enum GameState
        {
            TitleScreen,
            Playing,
            /// <summary>
            /// Freeze the game for a moment but still draw and play a jingle when you get a coin.
            /// </summary>
            GotCoin,
            /// <summary>
            /// Gameplay with some menu displaying. The menu will transition the state back.
            /// </summary>
            PausedWithMenu,
            /// <summary>
            /// Once you get 100 tacos the game will sort of pause while we reveal the 100 taco coin.
            /// </summary>
            RevealTacoCoin,
            /// <summary>
            /// Talking to an NPC or text coming up because of some action (trying to open a locked door, etc.)
            /// </summary>
            Conversation,
            Dead
        }
    }
}
