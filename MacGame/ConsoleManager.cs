using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MacGame
{
    public static class ConsoleManager
    {
        static StringBuilder Message = new StringBuilder();
        static string input = "";
        static KeyboardState previousKeyState;
        public static bool ShowConsole = false;
        static Player player;
        static ContentManager contentManager;
        static Game1 game;

        public static void Initialize(ContentManager contentManager, Player player, Game1 currentGame)
        {
            ConsoleManager.contentManager = contentManager;
            ConsoleManager.player = player;
            Message.Capacity = 200;
            Message.AppendLine("Welcome to the Console. If you know the secret keys you can type them here and do some magical things. Oh boy!");
            Message.Append(" >> ");
            game = currentGame;
        }

        public static void Update(float elapsed)
        {
            var keyState = Keyboard.GetState();

            if (ShowConsole)
            {
                foreach (var key in keyState.GetPressedKeys())
                {
                    if (previousKeyState.IsKeyUp(key))
                    {
                        if (key == Keys.Enter)
                        {
                            // Submit the message and get ready for a new one.
                            var response = ProcessInput(input);
                            Message.AppendLine(input);
                            Message.AppendLine(response);
                            Message.Append(" >> ");
                            input = "";
                        }
                        else if (key == Keys.Escape)
                        {
                            if (input.Length > 0)
                            {
                                // Clear the input if there is one
                                input = "";
                            }
                            else
                            {
                                // Get out of here
                                ShowConsole = false;
                            }
                        }
                        else if (key == Keys.Delete || key == Keys.Back)
                        {
                            // Delete a character
                            if (input.Length > 0)
                            {
                                input = input.Remove(input.Length - 1);
                            }
                        }
                        else
                        {
                            // Append the key they pressed to the input
                            if ((int)key > 47 && (int)key < 91) // Gross, hacky hack hack!!!
                            {
                                // These are the number and letter keys. Numbers values start with D, like D1, D2. so get rid of that.
                                string strKey = key.ToString();
                                strKey = strKey.Substring(strKey.Length - 1);
                                input += strKey;
                            }
                            else if (key == Keys.Space)
                            {
                                input += " ";
                            }
                        }
                    }
                }

                int maxLength = 200;
                if (Message.Length > maxLength)
                {
                    Message = Message.Remove(0, Message.Length - maxLength);
                }
            }
            else
            {
                //Check if they want to show the console
                if (Game1.IS_DEBUG && keyState.IsKeyDown(Keys.C))
                {
                    ShowConsole = true;
                }
            }
            previousKeyState = keyState;
        }

        private static string ProcessInput(string input)
        {
            input = input.Trim().ToLower();
            var inputArray = input.Split(" ".ToCharArray());
            if (input == "godmode" || input.StartsWith("godmode ") || input == "god" || input.StartsWith("god "))
            {
                throw new NotImplementedException("TODO");
                //player.MakeInvincible();

                //var item = new Everything(contentManager, 0, 0);
                //item.HandleItemCollision();

                //// check if they entered a level to warp to after
                //var array = input.Split(' ');
                //if (array.Length > 1)
                //{
                //    int level;
                //    int.TryParse(array[1], out level);
                //    if (level > 0)
                //    {
                //        LevelManager.LoadLevel(level);
                //    }
                //}
                //ShowConsole = false;
                //return "You are a golden god";
            }
            if (input == "fullhealth")
            {
                player.Health = Player.MaxHealth;
                return "Full Health Baby!!!!";
            }
            if (input == "invincible")
            {
                throw new NotImplementedException("TODO");
                //return "Invincible!!!!!!!!!!!!!!!!!!111111111eleven";
            }
            if (input == "exit")
            {
                ShowConsole = false;
            }
            if (input.StartsWith("goto "))
            {
                
                string mapName = input.Substring("goto ".Length);

                // check if it exists
                if (!File.Exists($@"Content\Maps\{mapName}.xnb"))
                {
                    return $"Map {mapName} not found.";
                }

                game.GoToLevel(mapName);
                return "Mape " + mapName + " Loaded Bro";
            }
            if (input == "fuckyou" || input == "fuck you")
            {
                return "Hey, fuck you buddy!";
            }
            // TODO: get powerups, get socks
         
            if (input.StartsWith("save "))
            {
                if (inputArray.Length >= 2)
                {
                    int saveSlot = 0;
                    if (int.TryParse(inputArray[1], out saveSlot))
                    {
                        StorageManager.TrySaveGame(saveSlot);
                        return "Saving in slot: " + saveSlot.ToString();
                    }
                }
            }
            if (input.StartsWith("load "))
            {
                if (inputArray.Length >= 2)
                {
                    int saveSlot = 0;
                    if (int.TryParse(inputArray[1], out saveSlot))
                    {
                        StorageManager.TryLoadGame(saveSlot);
                        return "Loading from slot: " + saveSlot.ToString();
                    }
                }
            }
            if (input.StartsWith("sock"))
            {
                int sockCount = 1;
                if (inputArray.Length >= 2)
                {
                    int.TryParse(inputArray[1], out sockCount);
                }
                // Add this manys socks
                foreach (var levelNumber in SockIndex.LevelNumberToSocks.Keys)
                {
                    // Load the state for that level to see if you already have the sock.
                    if (!Game1.StorageState.Levels.ContainsKey(levelNumber))
                    {
                        Game1.StorageState.Levels.Add(levelNumber, new LevelStorageState());
                    }
                    var levelState = Game1.StorageState.Levels[levelNumber];
                    foreach (var sock in SockIndex.LevelNumberToSocks[levelNumber])
                    {
                        if (!levelState.CollectedSocks.Contains(sock.Name))
                        {
                            levelState.CollectedSocks.Add(sock.Name);
                            player.SockCount++;
                            sockCount--;

                            if (sockCount <= 0)
                            {
                                return "Done!";
                            }
                        }
                    }
                }
            }
            if (input.StartsWith("breakbricks"))
            {
                foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (gameObject is BreakBrick)
                    {
                        var bb = (BreakBrick)gameObject;
                        bb.Break();
                    }
                }
                return "broked";
            }
            return "Message Not Recognized";
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.TileTextures, new Rectangle(0, 0, Game1.GAME_X_RESOLUTION, Game1.GAME_Y_RESOLUTION), Game1.WhiteSourceRect, Color.Black * 0.75f);

            if (ShowConsole)
            {
                spriteBatch.DrawString(Game1.Font, Message.ToString().Substring(0, Message.Length - 1) + input, Vector2.Zero, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            }
        }
    }
}
