using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MacGame
{
    public static class ConversationManager
    {

        // Dialog box background and border components
        static Rectangle borderCornerSourceRect;
        static Rectangle borderLeftEdgeSourceRect;
        static Rectangle borderTopEdgeSourceRect;
        static Rectangle dialogBoxBackgroundSourceRect;

        static Rectangle advanceMessageArrowSourceRect;

        const float textDepth = 0.1f;

        static int bubbleHeight;
        static int bubbleWidth;
        static int textWidth;
        static int textHeight;

        static float textScale = 1f;

        private static float letterTimer = 0f;
        private const float letterTimerGoal = 0.04f;
        private static int currentLetterIndex;
        private static int totalLetters;

        static string ChoicePointerString = " > ";
        static float ChoicePointerWidth = 0f;

        private static SpriteFont Font;

        /// <summary>
        /// Represents the face of the person that is talking.
        /// </summary>
        static Texture2D conversationTexture;
        
        public enum ImagePosition
        {
            Left, Right
        }

        public enum Float
        {
            Top,
            Bottom
        }

        public class ConversationChoice
        {
            public ConversationChoice(string text, System.Action action)
            {
                this.Text = text;
                this.Event = action;

                Width = Game1.Font.MeasureString(text).X * textScale;

            }
            public float Width { get; private set; }
            public string Text { get; private set; }
            public System.Action Event { get; set; }
        }

        public class ConversationMessage
        {
            public List<string> Texts;
            public ImagePosition ImagePosition;
            public Rectangle? ImageSourceRectangle;

            public int selectedChoice = 0;

            // An array of strings to show as choices after the last message.
            public List<ConversationChoice> Choices;

            public void PlaySound()
            {
                // TODO: Play sound
                //SoundManager.PlaySound("message");
            }
        }

        private static List<ConversationMessage> Messages = new List<ConversationMessage>();

        public static void AddMessage(string text, Rectangle? imageSource = null, ImagePosition imagePosition = ImagePosition.Left, List<ConversationChoice> choices = null)
        {
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
                lineWidth += 18; // Cheat a little bit to make it look better.
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
                lastMessage.Choices = choices;
            }

            if (isFirstMessage)
            {
                SetupNewMessage();
            }
        }

        private static void SetupNewMessage()
        {
            currentLetterIndex = 0;
            letterTimer = 0;
            totalLetters = 0;

            if (Messages.Count > 0)
            {
                Messages[0].PlaySound();
                foreach (var text in Messages[0].Texts)
                {
                    totalLetters += text.Length;
                }
            }
        }

        public static void Initialize(ContentManager content)
        {
            bubbleWidth = 14 * Game1.TileSize;
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
        }

        public static bool IsInConversation()
        {
            return Messages.Count > 0;
        }

        public static void Update(float elapsed)
        {
            if (Messages.Count == 0) return;

            var player = Game1.Player;

            var pa = player.InputManager.PreviousAction;
            var ca = player.InputManager.CurrentAction;

            // if enough time has passed and we dont need to block the inputs any more, see if they pressed the button
            // if so, advance through the messages.
            if (currentLetterIndex >= totalLetters)
            {
                if (ca.acceptMenu && !pa.acceptMenu)
                {
                    var message = Messages[0];

                    if (message.Choices != null)
                    {
                        var action = message.Choices[message.selectedChoice].Event;
                        if (action != null)
                        {
                            action.Invoke();
                        }
                    }

                    Messages.RemoveAt(0);
                    SetupNewMessage();
                }
            }

            if (Messages.Count == 0) return;

            // Let them choose a choice
            var currentMessage = Messages[0];
            if (currentMessage.Choices != null)
            {
                if (ca.left && !pa.left)
                {
                    currentMessage.selectedChoice--;
                    if (currentMessage.selectedChoice < 0)
                    {
                        currentMessage.selectedChoice = currentMessage.Choices.Count - 1;
                    }
                }
                else if (ca.right && !pa.right)
                {
                    currentMessage.selectedChoice++;
                    if (currentMessage.selectedChoice >= currentMessage.Choices.Count)
                    {
                        currentMessage.selectedChoice = 0;
                    }
                }
            }

            float speedMultiplier = 2f;
            if (ca.acceptMenu)
            {
                speedMultiplier = 4f;
            }
            letterTimer += elapsed * speedMultiplier;

            if (letterTimer >= letterTimerGoal && currentLetterIndex < totalLetters)
            {
                currentLetterIndex++;
                if (currentLetterIndex % 3 == 0)
                {
                    // TODO: Sound for this
                    //SoundManager.PlaySound("menumove", 0.35f, 0.5f, 0.5f);
                }
                letterTimer -= letterTimerGoal;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Messages.Count == 0) return;

            var currentMessage = Messages[0];

            int leftMargin = (Game1.GAME_X_RESOLUTION - bubbleWidth) / 2;
            int topMargin;


            // float to top or bottom depending on Mac's position.
            Float @float = Float.Bottom;

            // Display the message on the top if Mac is near the bottom of the screen.
            if (Game1.Player.WorldLocation.Y > Game1.Camera.Position.Y + Game1.TileSize)
            {
                @float = Float.Top;
            }


            switch (@float)
            {
                case Float.Top:
                    topMargin = 60;
                    break;
                case Float.Bottom:
                    topMargin = Game1.GAME_Y_RESOLUTION - bubbleHeight - 40;
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
                        personXOffset = Game1.TileSize + 10;
                        personSpriteEffect = SpriteEffects.None;
                        break;
                    case ImagePosition.Right:
                        personXOffset = Game1.GAME_X_RESOLUTION - currentMessage.ImageSourceRectangle.Value.Width - Game1.TileSize - 18;
                        personSpriteEffect = SpriteEffects.FlipHorizontally;
                        break;
                    default:
                        throw new Exception("Image not supported");
                }
            }


            Color borderColor = Color.White;

            // Draw the dialog box
            var tileWidth = bubbleWidth / Game1.TileSize;
            var tileHeight = bubbleHeight / Game1.TileSize;

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

                    spriteBatch.Draw(Game1.TileTextures, new Rectangle(leftMargin + i * Game1.TileSize, topMargin + (j * Game1.TileSize) + 2, Game1.TileSize, Game1.TileSize), sourceRect, borderColor, 0f, Vector2.Zero, dialogBoxSpriteEffect, 0f);
                }
            }

            int arrowX = leftMargin + bubbleWidth - advanceMessageArrowSourceRect.Width - 8;

            int textLeftMargin = leftMargin;
            if (currentMessage.ImagePosition == ImagePosition.Left && currentMessage.ImageSourceRectangle.HasValue)
            {
                textLeftMargin += currentMessage.ImageSourceRectangle.Value.Width;
            }

            // draw the text
            DrawTexts(spriteBatch, currentMessage.Texts, new Vector2(textLeftMargin + Game1.TileSize, topMargin + 22), textScale, textDepth, currentLetterIndex);

            // Draw the choices. Start in the bottom right corner
            var location = new Vector2(textLeftMargin + bubbleWidth, topMargin + bubbleHeight);
            if (currentMessage.Choices != null)
            {

                // Move the location back the width of all choices.
                var totalWidth = 0f;
                foreach (var choice in currentMessage.Choices)
                {
                    totalWidth += choice.Width;
                    totalWidth += ChoicePointerWidth;
                }
                location.X -= totalWidth;

                for (int i = 0; i < currentMessage.Choices.Count; i++)
                {
                    var choice = currentMessage.Choices[i];
                    bool isSelected = i == currentMessage.selectedChoice;
                    if (isSelected)
                    {
                        spriteBatch.DrawString(Game1.Font, ChoicePointerString, location, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, textDepth);
                    }
                    location.X += ChoicePointerWidth;
                    spriteBatch.DrawString(Game1.Font, choice.Text, location, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, textDepth);
                    location.X += choice.Width;
                }
            }

            // draw the advance the text arrow
            if (Messages.Count > 1 && currentLetterIndex >= totalLetters)
            {
                spriteBatch.Draw(Game1.TileTextures, new Vector2(arrowX, topMargin + bubbleHeight - advanceMessageArrowSourceRect.Height - 2), advanceMessageArrowSourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, textDepth);
            }

            // draw the image of the person talking. We expect conversation texture to be a spritesheet of squares so we'll use height for everything.
            if (currentMessage.ImageSourceRectangle != null)
            {
                spriteBatch.Draw(conversationTexture, new Vector2(personXOffset, topMargin + Game1.TileSize + 16), currentMessage.ImageSourceRectangle.Value, Color.White, 0f, Vector2.Zero, 1f, personSpriteEffect, textDepth);
            }
        }

        /// <summary>
        /// Draw a list of strings one of top of each other.
        /// </summary>
        private static void DrawTexts(SpriteBatch spriteBatch, List<string> strings, Vector2 position, float scale, float depth, int maxLetters = int.MaxValue)
        {
            // We could use the font height but instead this game will only work with TileSize height fonts.
            var wordHeight = Game1.TileSize + 4;

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
                spriteBatch.DrawString(Font, lineToDraw, drawLocation, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, depth);
                drawLocation.Y += wordHeight;
                if (previousLinesLetterCount > maxLetters)
                {
                    return;
                }
            }
        }


        /// <summary>
        /// Draw text at some location wrapped based on maxWidth.
        /// </summary>
        private static void DrawWrappedText(SpriteBatch spriteBatch, string text, int maxWidth, Vector2 position, float scale, float depth)
        {
            var strings = GetLineWrappedText(text, maxWidth, scale);
            DrawTexts(spriteBatch, strings, position, scale, depth);
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

            foreach (var paragraph in paragraphs)
            {
                var words = paragraph.Split(" ".ToCharArray());
                foreach (string word in words)
                {
                    var wordLength = Font.MeasureString(word + " ").X * scale;
                    remainingLineSpace -= wordLength;
                    if (remainingLineSpace > 0)
                    {
                        currentLine += word + " ";
                    }
                    else
                    {
                        strings.Add(currentLine);
                        remainingLineSpace = (float)maxWidth - wordLength;
                        currentLine = word + " ";
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
    }
}
