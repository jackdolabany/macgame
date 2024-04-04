using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MacGame;

namespace MacGame
{
    public static class ConversationManager
    {

        /// <summary>
        /// All of the components that we'll need to draw to create a menu block
        /// </summary>
        //static Rectangle cornerSourceRect;
        //static Rectangle blueSourceRect;
        //static Rectangle horizontalEdgeSourceRect;
        //static Rectangle verticalEdgeSourceRect;
        //static Rectangle advanceMessageArrowSourceRect;

        const float textDepth = 0.1f;
        //const float bubbleDepth = 0.2f;

        static int bubbleHeight;
        static int bubbleWidth;
        //static int personPictureScaledWidth;
        //static float personPictureScale;
        static int textWidth;

        static float textScale = 1f;

        private static float letterTimer = 0f;
        private const float letterTimerGoal = 0.04f;
        private static int currentLetterIndex;
        private static int totalLetters;

        static string ChoicePointerString = " > ";
        static float ChoicePointerWidth = 0f;

        private static SpriteFont Font;
        private static Vector2 FontSize;

        /// <summary>
        /// Represents the face of the person that is talking.
        /// </summary>
        //static Texture2D conversationTexture;

        //public enum Image
        //{
        //    Player,
        //    Fishhead,
        //    Kiosk,
        //    ShopKeep
        //}

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
            public List<string> Text;
            //public Image Image;
            public Float Float;

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

        public static void AddMessage(string text, Float @float, List<ConversationChoice> choices = null)
        {
            bool isFirstMessage = false;
            if (Messages.Count == 0)
            {
                isFirstMessage = true;
            }

            // Calculate the number of lines we can display.
            var wordHeight = FontSize.Y * textScale;
            var linesToDisplay = (int)Math.Floor((float)bubbleHeight / wordHeight);

            var lines = GetLineWrappedText(text, textWidth, textScale);

            ConversationMessage lastMessage = null;

            // each add may create multiple messages if there is enough text.
            for (int i = 0; i < lines.Count; i += linesToDisplay)
            {
                var currentMessage = new ConversationMessage() { Text = new List<string>(), Float = @float };
                lastMessage = currentMessage;
                Messages.Add(currentMessage);
                for (int j = 0; j < linesToDisplay && i + j < lines.Count; j++)
                {
                    currentMessage.Text.Add(lines[i + j]);
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
                foreach (var text in Messages[0].Text)
                {
                    totalLetters += text.Length;
                }
            }
        }

        public static void Initialize(ContentManager content)
        {
            bubbleWidth = 100;
            bubbleHeight = 35;

            textWidth = bubbleWidth;

            ChoicePointerWidth = Game1.Font.MeasureString(ChoicePointerString).X * textScale;

            Font = Game1.Font;
            FontSize = Font.MeasureString("A");
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
            int bottomMargin = leftMargin;
            int topMargin = Game1.GAME_Y_RESOLUTION - bubbleHeight - bottomMargin;

            ////     draw the conversation bubble
            //// background filler. Edges are handled below     
            //int offset = 24;
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + offset, topMargin + offset, bubbleWidth - 2 * offset, bubbleHeight - 2 * offset), blueSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, bubbleDepth);

            ////     Corners
            //// top left corner
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin, topMargin, cornerSourceRect.Width, cornerSourceRect.Height), cornerSourceRect, Color.White, 0f, new Vector2(cornerSourceRect.Width / 2, cornerSourceRect.Height / 2), SpriteEffects.None, bubbleDepth);

            //// top right corner
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + bubbleWidth, topMargin, cornerSourceRect.Width, cornerSourceRect.Height), cornerSourceRect, Color.White, MathHelper.PiOver2, new Vector2(cornerSourceRect.Width / 2, cornerSourceRect.Height / 2), SpriteEffects.None, bubbleDepth);

            //// bottom right corner
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + bubbleWidth, topMargin + bubbleHeight, cornerSourceRect.Width, cornerSourceRect.Height), cornerSourceRect, Color.White, MathHelper.Pi, new Vector2(cornerSourceRect.Width / 2, cornerSourceRect.Height / 2), SpriteEffects.None, bubbleDepth);

            //// bottom left corner
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin, topMargin + bubbleHeight, cornerSourceRect.Width, cornerSourceRect.Height), cornerSourceRect, Color.White, MathHelper.Pi + MathHelper.PiOver2, new Vector2(cornerSourceRect.Width / 2, cornerSourceRect.Height / 2), SpriteEffects.None, bubbleDepth);

            ////    Between the corner fillers
            //// top filler
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + (cornerSourceRect.Width / 2), topMargin - (verticalEdgeSourceRect.Height / 2), bubbleWidth - cornerSourceRect.Width, verticalEdgeSourceRect.Height), verticalEdgeSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, bubbleDepth);

            //// bottom filler
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + (cornerSourceRect.Width / 2), topMargin + bubbleHeight - (verticalEdgeSourceRect.Height / 2), bubbleWidth - cornerSourceRect.Width, verticalEdgeSourceRect.Height), verticalEdgeSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, bubbleDepth);

            //// left filler
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin - (cornerSourceRect.Width / 2), topMargin + (cornerSourceRect.Height / 2), horizontalEdgeSourceRect.Width, bubbleHeight - cornerSourceRect.Height), horizontalEdgeSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, bubbleDepth);

            //// right filler
            //spriteBatch.Draw(menuTexture, new Rectangle(leftMargin + bubbleWidth - (cornerSourceRect.Width / 2), topMargin + (cornerSourceRect.Height / 2), horizontalEdgeSourceRect.Width, bubbleHeight - cornerSourceRect.Height), horizontalEdgeSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, bubbleDepth);

            //    The stuff over the bubble
            //Rectangle personSourceRect = new Rectangle(((int)currentMessage.Image) * conversationTexture.Height, 0, conversationTexture.Height, conversationTexture.Height);
            //Vector2 personImageOffset = new Vector2(20, -2);
            //int personXOffset = 0;
            //SpriteEffects personSpriteEffect;
            //int textXOffet = leftMargin;
            //int arrowX = leftMargin + bubbleWidth - advanceMessageArrowSourceRect.Width + 20;

            //switch (currentMessage.Float)
            //{
            //    case Float.Left:
            //        personSpriteEffect = SpriteEffects.None;
            //        textXOffet += personPictureScaledWidth;
            //        personXOffset = leftMargin - (int)personImageOffset.X;
            //        break;
            //    case Float.Right:
            //        personSpriteEffect = SpriteEffects.FlipHorizontally;
            //        personXOffset = (int)(leftMargin - personPictureScaledWidth + bubbleWidth + personImageOffset.X);
            //        arrowX -= personPictureScaledWidth;
            //        textXOffet += 25;
            //        break;
            //    default:
            //        throw new Exception("Float not supported");

            //}


            switch (currentMessage.Float)
            {
                case Float.Top:
                    topMargin = 10;
                    break;
                case Float.Bottom:
                    topMargin = Game1.GAME_Y_RESOLUTION - bubbleHeight - 10;
                    break;
                default:
                    throw new Exception("Float not supported");
            }

            // draw the text
            DrawTexts(spriteBatch, currentMessage.Text, new Vector2(leftMargin, topMargin + 10), textScale, textDepth, currentLetterIndex);

            // Draw the choices. Start in the bottom right corner
            var location = new Vector2(leftMargin + bubbleWidth, topMargin + bubbleHeight);
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

                // Hack it up a bit
                location += new Vector2(-90, -85);

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

            //// draw the advance the text arrow
            //if (Messages.Count > 1 && currentLetterIndex >= totalLetters)
            //{
            //    spriteBatch.Draw(menuTexture, new Vector2(arrowX, topMargin + bubbleHeight - advanceMessageArrowSourceRect.Height + 25), advanceMessageArrowSourceRect, Color.White, 0f, advanceMessageArrowSourceRect.RelativeCenterVector(), 0.5f, SpriteEffects.None, textDepth);
            //}

            //// draw the image of the person talking. We expect conversation texture to be a spritesheet of squares so we'll use height for everything.
            //spriteBatch.Draw(conversationTexture, new Vector2(personXOffset, topMargin + bubbleHeight - (personSourceRect.Height * personPictureScale) + (int)personImageOffset.Y), personSourceRect, Color.White, 0f, Vector2.Zero, personPictureScale, personSpriteEffect, textDepth);

        }

        /// <summary>
        /// Draw a list of strings one of top of each other.
        /// </summary>
        private static void DrawTexts(SpriteBatch spriteBatch, List<string> strings, Vector2 position, float scale, float depth, int maxLetters = int.MaxValue)
        {
            var wordHeight = FontSize.Y * scale;
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
