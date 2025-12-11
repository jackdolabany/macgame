using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;
using System.Linq;

namespace MacGame
{
    public static class ConversationManager
    {
        public enum ImagePosition
        {
            Left, Right
        }

        public enum Float
        {
            Top,
            Bottom
        }

        public static Rectangle PlayerSourceRectangle;

        // Dialog box background and border components
        static Rectangle borderCornerSourceRect;
        static Rectangle borderLeftEdgeSourceRect;
        static Rectangle borderTopEdgeSourceRect;
        static Rectangle dialogBoxBackgroundSourceRect;

        static Rectangle advanceMessageArrowSourceRect;

        const float textDepth = TileMap.OVERLAY_DRAW_DEPTH;

        static int bubbleHeight;
        static int bubbleWidth;
        static int textWidth;
        static int textHeight;

        public static float textScale = Game1.FontScale;

        private static float letterTimer = 0f;
        private const float letterTimerGoal = 0.04f;
        private static int currentLetterIndex;
        private static int totalLetters;

        public static bool IsMessageFullyDisplayed => currentLetterIndex > 0 && currentLetterIndex == totalLetters;

        static string ChoicePointerString = " > ";
        static float ChoicePointerWidth = 0f;

        private static SpriteFont Font;

        /// <summary>
        /// Represents the face of the person that is talking.
        /// </summary>
        static Texture2D conversationTexture;

        private static List<ConversationMessage> Messages = new List<ConversationMessage>();

        /// <summary>
        /// When you add a message we figure out if we should show it at the top or bottom of the screen.
        /// </summary>
        private static Float _float;

        /// <summary>
        /// Conversations can pause gameplay or just pop up over the gameplay and
        /// the text will advance after some time.
        /// </summary>
        private static bool _pauseGameplay = false;

        /// <summary>
        /// If the message doesn't pause gameplay the messages will advance after some time.
        /// </summary>
        private static float _showMessageTimeRemaining = 0f;
        private const float _messageTime = 3f;

        // Have a small delay where you just see the black box before we start typing letters.
        public static float typeDelayTimer = 0f;
        public const float typeDelayTimerGoal = 0.3f;

        // Small delay before you can select a choice so that you don't click it accidentally before you see it.
        public static float choiceDelayTimer = 0f;
        public const float choiceDelayTimerGoal = 0.3f;

        public static void AddMessage(
            string text, 
            Rectangle? imageSource = null, 
            ImagePosition imagePosition = ImagePosition.Left, 
            List<ConversationChoice>? choices = null, 
            System.Action? completeAction = null,
            bool pauseGameplay = true)
        {
            _pauseGameplay = pauseGameplay;

            if (!_pauseGameplay)
            {
                _showMessageTimeRemaining = _messageTime;
            }    

            // float to top or bottom depending on Mac's position.
            _float = Float.Bottom;

            // Display the message on the top if Mac is near the bottom of the screen.
            if (Game1.Player.WorldLocation.Y > Game1.Camera.Position.Y + Game1.TileSize)
            {
                _float = Float.Top;
            }

            bool isFirstMessage = false;
            if (Messages.Count == 0)
            {
                isFirstMessage = true;
            }

            // Calculate the number of lines we can display.
            var wordHeight = Game1.TileSize; // Game will only work with tile size fonts.
            var linesToDisplay = (int)((float)textHeight / (float)wordHeight);

            var lineWidth = textWidth;
            if (imageSource != null)
            {
                lineWidth -= imageSource.Value.Width;
                lineWidth += 32; // Cheat a little bit to make it look better.
            }

            var lines = GetLineWrappedText(text, lineWidth, textScale);

            ConversationMessage lastMessage = null;

            // each add may create multiple messages if there is enough text.
            for (int i = 0; i < lines.Count; i += linesToDisplay)
            {
                var currentMessage = new ConversationMessage() { Texts = new List<string>(), ImagePosition = imagePosition, ImageSourceRectangle = imageSource };
                lastMessage = currentMessage;
                Messages.Add(currentMessage);
                for (int j = 0; j < linesToDisplay && i + j < lines.Count; j++)
                {
                    currentMessage.Texts.Add(lines[i + j]);
                }
            }

            if (choices != null)
            {

                if (lastMessage == null)
                {
                    var currentMessage = new ConversationMessage() { Texts = new List<string>(), ImagePosition = imagePosition, ImageSourceRectangle = imageSource };
                    lastMessage = currentMessage;
                    Messages.Add(currentMessage);
                }

                lastMessage.Choices.AddRange(choices);
            }

            lastMessage.CompleteAction = completeAction;

            if (isFirstMessage)
            {
                SetupNewMessage();
                Messages[0].PlaySound();
            }
        }

        private static void SetupNewMessage()
        {
            currentLetterIndex = 0;
            letterTimer = 0;
            totalLetters = 0;

            typeDelayTimer = 0f;
            choiceDelayTimer = 0f;

            if (Messages.Count > 0)
            {
                foreach (var text in Messages[0].Texts)
                {
                    totalLetters += text.Length;
                }
            }
        }

        public static void Initialize(ContentManager content)
        {
            // Controls how many 8x8 blocks the text bubble is.
            bubbleWidth = 16 * Game1.TileSize;
            bubbleHeight = 5 * Game1.TileSize;

            textWidth = bubbleWidth - 2 * Game1.TileSize;
            textHeight = bubbleHeight - 2 * Game1.TileSize;

            ChoicePointerWidth = Game1.Font.MeasureString(ChoicePointerString).X * textScale;

            Font = Game1.Font;

            borderCornerSourceRect = Helpers.GetTileRect(0, 11);
            borderLeftEdgeSourceRect = Helpers.GetTileRect(0, 12);
            borderTopEdgeSourceRect = Helpers.GetTileRect(1, 11);
            dialogBoxBackgroundSourceRect = Helpers.GetTileRect(1, 12);
            advanceMessageArrowSourceRect = Helpers.GetTileRect(0, 14);

            conversationTexture = content.Load<Texture2D>(@"Textures\ReallyBigTextures");

            PlayerSourceRectangle = Helpers.GetReallyBigTileRect(0, 0);
        }

        public static bool ShouldPauseForConversation()
        {
            return Messages.Count > 0 && _pauseGameplay;
        }

        public static void Update(float elapsed)
        {
            if (Messages.Count == 0) return;

            if (_showMessageTimeRemaining > 0)
            {
                _showMessageTimeRemaining -= elapsed;
            }

            // This adds a little delay before the letters start typing
            // out to the screen.
            typeDelayTimer += elapsed;

            if (typeDelayTimer < typeDelayTimerGoal)
            {
                return;
            }

            // Read inputs.
            var player = Game1.Player;
            var pa = player.InputManager.PreviousAction;
            var ca = player.InputManager.CurrentAction;

            // Advance the letters.
            float letterSpeed = 0.8f;
            if (ca.acceptMenu)
            {
                // Advance faster if they hold the button down.
                letterSpeed *= 2;
            }
            letterTimer += elapsed * letterSpeed;

            if (letterTimer >= letterTimerGoal && currentLetterIndex < totalLetters)
            {
                currentLetterIndex++;
                if (currentLetterIndex % 3 == 0)
                {
                    SoundManager.PlaySound("TypeLetter", 0.35f, 0.5f);
                }
                letterTimer -= letterTimerGoal;
            }

            if (IsMessageFullyDisplayed && choiceDelayTimer <= choiceDelayTimerGoal)
            {
                choiceDelayTimer += elapsed;
            }

            bool canMessageBeAdvanced;  
            if (Messages[0].Choices.Any())
            {
                canMessageBeAdvanced = IsMessageFullyDisplayed && choiceDelayTimer >= choiceDelayTimerGoal;
            }
            else
            {
                canMessageBeAdvanced = IsMessageFullyDisplayed;
            }

            // Handle inputs
            if (canMessageBeAdvanced)
            {
                var pressedAcceptButton = ca.acceptMenu && !pa.acceptMenu;

                // Execute the action if they pressed the button
                if ((pressedAcceptButton && _pauseGameplay) || (_showMessageTimeRemaining <= 0 && !_pauseGameplay))
                {
                    var message = Messages[0];

                    if (message.Choices.Any())
                    {
                        var action = message.Choices[message.selectedChoice].Event;
                        if (action != null)
                        {
                            action.Invoke();
                        }
                    }

                    if (message.CompleteAction != null)
                    {
                        message.CompleteAction.Invoke();
                    }

                    Messages.RemoveAt(0);
                    SetupNewMessage();

                    _showMessageTimeRemaining = _messageTime;
                    return;
                }

                // Move the current selection up or down.
                var currentMessage = Messages[0];
                if (currentMessage.Choices != null)
                {
                    if (ca.up && !pa.up)
                    {
                        currentMessage.selectedChoice--;
                        if (currentMessage.selectedChoice < 0)
                        {
                            currentMessage.selectedChoice = currentMessage.Choices.Count - 1;
                        }
                    }
                    else if (ca.down && !pa.down)
                    {
                        currentMessage.selectedChoice++;
                        if (currentMessage.selectedChoice >= currentMessage.Choices.Count)
                        {
                            currentMessage.selectedChoice = 0;
                        }

                    }
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Messages.Count == 0) return;

            var currentMessage = Messages[0];

            int leftMargin = (Game1.GAME_X_RESOLUTION - bubbleWidth) / 2;
            int topMargin;

            switch (_float)
            {
                case Float.Top:
                    topMargin = Game1.TileSize;
                    break;
                case Float.Bottom:
                    topMargin = Game1.GAME_Y_RESOLUTION - bubbleHeight - Game1.TileSize;
                    break;
                default:
                    throw new Exception("Float not supported");
            }

            SpriteEffects personSpriteEffect = SpriteEffects.None;
            int personXOffset = 0;

            if (currentMessage.ImageSourceRectangle != null)
            {
                switch (currentMessage.ImagePosition)
                {
                    case ImagePosition.Left:
                        personXOffset = leftMargin + 10;
                        personSpriteEffect = SpriteEffects.None;
                        break;
                    case ImagePosition.Right:
                        personXOffset = leftMargin - 18 + bubbleWidth - currentMessage.ImageSourceRectangle.Value.Width;
                        personSpriteEffect = SpriteEffects.FlipHorizontally;
                        break;
                    default:
                        throw new Exception("Image not supported");
                }
            }

            // Draw the dialog box
            var tileWidth = bubbleWidth / Game1.TileSize;
            var tileHeight = bubbleHeight / Game1.TileSize;

            DrawDialogBox(spriteBatch, new Vector2(leftMargin, topMargin), tileWidth, tileHeight, 0f);

            int arrowX = leftMargin + bubbleWidth - advanceMessageArrowSourceRect.Width - 8;
           
            // Move the text advance arrow to the left if there's an image there.
            if (currentMessage.ImagePosition == ImagePosition.Right && currentMessage.ImageSourceRectangle.HasValue)
            {
                arrowX -= currentMessage.ImageSourceRectangle.Value.Width;
            }

            int textLeftMargin = leftMargin;
            if (currentMessage.ImagePosition == ImagePosition.Left && currentMessage.ImageSourceRectangle.HasValue)
            {
                textLeftMargin += currentMessage.ImageSourceRectangle.Value.Width;
            }

            // We could use the font height but instead this game will only work with TileSize height fonts.
            var wordHeight = Game1.TileSize + 4;

            // draw the text
            DrawTexts(spriteBatch, currentMessage.Texts, new Vector2(textLeftMargin + 24, topMargin + 26), textScale, textDepth, wordHeight, currentLetterIndex);

            // Draw the choices.
            var location = new Vector2(textLeftMargin + 16, topMargin + wordHeight + wordHeight - 8);

            if (currentMessage.Choices.Any() && IsMessageFullyDisplayed)
            {

                for (int i = 0; i < currentMessage.Choices.Count; i++)
                {
                    var choice = currentMessage.Choices[i];
                    bool isSelected = i == currentMessage.selectedChoice;
                    if (isSelected)
                    {
                        spriteBatch.DrawString(Game1.Font, ChoicePointerString, location, Pallette.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, textDepth);
                    }
                    
                    spriteBatch.DrawString(Game1.Font, choice.Text, location + new Vector2(ChoicePointerWidth, 0), Pallette.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, textDepth);
                    location.Y += wordHeight;
                }
            }

            // draw the advance the text arrow if they can press the button to advance text.
            if (_pauseGameplay && Messages.Count > 1 && currentLetterIndex >= totalLetters)
            {
                spriteBatch.Draw(Game1.TileTextures, new Vector2(arrowX, topMargin + bubbleHeight - advanceMessageArrowSourceRect.Height - 2), advanceMessageArrowSourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, textDepth);
            }

            // draw the image of the person talking. We expect conversation texture to be a spritesheet of squares so we'll use height for everything.
            if (currentMessage.ImageSourceRectangle != null)
            {
                spriteBatch.Draw(conversationTexture, new Vector2(personXOffset, topMargin + Game1.TileSize + 16), currentMessage.ImageSourceRectangle.Value, Color.White, 0f, Vector2.Zero, 1f, personSpriteEffect, textDepth);
            }
        }

        public static void DrawDialogBox(SpriteBatch spriteBatch, Vector2 location, int tileWidth, int tileHeight, float drawDepth)
        {
            for (int i = 0; i < tileWidth; i++)
            {
                for (int j = 0; j < tileHeight; j++)
                {
                    var dialogBoxSpriteEffect = SpriteEffects.None;
                    var sourceRect = dialogBoxBackgroundSourceRect;
                    if (i == 0 && j == 0)
                    {
                        // top left corner
                        sourceRect = borderCornerSourceRect;
                    }
                    else if (i == 0 && j == tileHeight - 1)
                    {
                        // bottom left corner
                        sourceRect = borderCornerSourceRect;
                        dialogBoxSpriteEffect = SpriteEffects.FlipVertically;
                    }
                    else if (i == tileWidth - 1 && j == 0)
                    {
                        // top right corner
                        sourceRect = borderCornerSourceRect;
                        dialogBoxSpriteEffect = SpriteEffects.FlipHorizontally;
                    }
                    else if (i == tileWidth - 1 && j == tileHeight - 1)
                    {
                        // bottom right corner
                        sourceRect = borderCornerSourceRect;
                        dialogBoxSpriteEffect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                    }
                    else if (i == tileWidth - 1)
                    {
                        // Right edge
                        sourceRect = borderLeftEdgeSourceRect;
                        dialogBoxSpriteEffect = SpriteEffects.FlipHorizontally;
                    }
                    else if (j == tileHeight - 1)
                    {
                        // bottom edge
                        sourceRect = borderTopEdgeSourceRect;
                        dialogBoxSpriteEffect = SpriteEffects.FlipVertically;
                    }
                    else if (i == 0)
                    {
                        sourceRect = borderLeftEdgeSourceRect;
                    }
                    else if (j == 0)
                    {
                        sourceRect = borderTopEdgeSourceRect;
                    }

                    spriteBatch.Draw(Game1.TileTextures, new Rectangle((int)location.X + i * Game1.TileSize, (int)location.Y + (j * Game1.TileSize) + 2, Game1.TileSize, Game1.TileSize), sourceRect, Color.White, 0f, Vector2.Zero, dialogBoxSpriteEffect, drawDepth);
                }
            }
        }

        /// <summary>
        /// Draw a list of strings one of top of each other.
        /// </summary>
        private static void DrawTexts(SpriteBatch spriteBatch, List<string> strings, Vector2 position, float scale, float depth, int wordHeight, int maxLetters = int.MaxValue)
        {
            Vector2 drawLocation = position;

            int previousLinesLetterCount = 0;

            foreach (var currentLine in strings)
            {
                var lineToDraw = currentLine;
                if ((previousLinesLetterCount + currentLine.Length) > maxLetters)
                {
                    lineToDraw = currentLine.Substring(0, (int)MathHelper.Min(maxLetters - previousLinesLetterCount, currentLine.Length));
                }
                previousLinesLetterCount += currentLine.Length;
                spriteBatch.DrawString(Font, lineToDraw, drawLocation, Pallette.White, 0f, Vector2.Zero, scale, SpriteEffects.None, depth);
                drawLocation.Y += wordHeight + 4;
                if (previousLinesLetterCount > maxLetters)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Takes in a string and returns a list of lines broken by the maxWidth.
        /// </summary>
        private static List<string> GetLineWrappedText(string text, int maxWidth, float scale)
        {
            string currentLine = "";
            float remainingLineSpace = (float)maxWidth;

            var strings = new List<string>(5);

            var paragraphs = text.Split("\n".ToCharArray());

            var spaceLength = Font.MeasureString(" ").X * scale;

            foreach (var paragraph in paragraphs)
            {
                var words = paragraph.Split(" ".ToCharArray());
                foreach (string word in words)
                {
                    var wordLength = Font.MeasureString(word).X * scale;
                    var isFirstWord = currentLine == "";
                    if (!isFirstWord)
                    {
                        // Need a space too if it's not the first word.
                        wordLength += spaceLength;
                    }

                    remainingLineSpace -= wordLength;

                    if (remainingLineSpace > 0 || isFirstWord)
                    {
                        if (isFirstWord)
                        {
                            currentLine = word;
                        }
                        else
                        {
                            currentLine += " " + word;
                        }
                    }
                    else
                    {
                        strings.Add(currentLine);
                        remainingLineSpace = (float)maxWidth - wordLength;
                        currentLine = word;
                    }
                }

                if (currentLine != "")
                {
                    strings.Add(currentLine);
                }
                remainingLineSpace = (float)maxWidth;
                currentLine = "";
            }
            return strings;
        }

        internal static void Clear()
        {
            Messages.Clear();
        }
    }

    public class ConversationChoice
    {
        public ConversationChoice(string text, System.Action? action)
        {
            this.Text = text;
            this.Event = action;

            Width = Game1.Font.MeasureString(text).X * ConversationManager.textScale;

        }
        public float Width { get; private set; }
        public string Text { get; private set; }
        public System.Action? Event { get; set; }
    }

    public class ConversationMessage
    {
        public List<string> Texts;
        public ConversationManager.ImagePosition ImagePosition;
        public Rectangle? ImageSourceRectangle;

        public int selectedChoice = 0;

        // An array of strings to show as choices after the last message.
        public List<ConversationChoice> Choices = new List<ConversationChoice>(5);

        /// <summary>
        /// Some kind of custom action to execute after the last message.
        /// </summary>
        public System.Action? CompleteAction { get; set; }

        public void PlaySound()
        {
        }
    }
}
