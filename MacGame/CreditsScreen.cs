using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace MacGame
{

    public static class CreditsScreen
    {

        private class Credit
        {
            public Credit(string title)
            {
                this.Title = title;
                this.People = new List<string>();
            }

            public string Title { get; private set; }
            public List<string> People { get; private set; }

            public void AddPerson(string name)
            {
                People.Add(name);
            }
        }

        static float positionX;
        static float positionY;
        static float velocity = 500;

        /// <summary>
        /// Each credit is show individusally. So a title
        /// followed by a list of people. Then onto the next.
        /// This is a list of lines to show for each credit.
        /// </summary>
        static List<List<string>> Lines;

        static int lineIndex;
        static float pauseTimer;
        const float pauseTimerGoal = 2f;

        static bool creditsAreDone;

        static int percentComplete;

        public static void Initialize()
        {
            var credits = new List<Credit>();

            credits.Add(new Credit("Executive Producer"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Game Design"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Story"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Artwork"));
            credits.Last().AddPerson("Sophia Dolabany");
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Programming"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Marketing"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Quality Assurance"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Music"));
            credits.Last().AddPerson("TBD");
            credits.Add(new Credit("Sound Effects"));
            credits.Last().AddPerson("Jack Dolabany");
            credits.Add(new Credit("Special Thanks"));
            credits.Last().AddPerson("Jack Frost Dolabany");
            credits.Last().AddPerson("Kayla Dolabany");

            positionX = Game1.GAME_X_RESOLUTION / 2;
            positionY = Game1.GAME_Y_RESOLUTION + 100;

            Lines = new List<List<string>>();

            foreach (var credit in credits)
            {
                var creditLines = new List<string>();
                Lines.Add(creditLines);

                creditLines.Add(credit.Title);
                foreach (var person in credit.People)
                {
                    creditLines.Add(person);
                }
            }

            lineIndex = 0;
            // TODO: Play credits music
            pauseTimer = 0f;

            percentComplete = Game1.StorageState.GetPercentComplete();

            var totalTime = Game1.StorageState.GetFormattedPlayTime();
            var statsLines = new List<string>();
            statsLines.Add("The End");
            statsLines.Add(" ");
            statsLines.Add($"Time: {totalTime}");
            statsLines.Add($"Socks: {percentComplete.ToString("00")}%");
            Lines.Add(statsLines);

            creditsAreDone = false;
        }

        public static void Update(float elapsed)
        {

            if (positionY <= Game1.GAME_Y_RESOLUTION / 2)
            {
                pauseTimer += elapsed;
            }

            if (positionY > Game1.GAME_Y_RESOLUTION / 2 || pauseTimer > pauseTimerGoal)
            {
                positionY -= elapsed * velocity;
            }

            int minY = -100;
            if (lineIndex == Lines.Count - 1)
            {
                // the end should show halfway up the screen and stay there.
                minY = Game1.GAME_Y_RESOLUTION / 2;
            }

            if (positionY < minY)
            {
                positionY = minY;
                if (lineIndex < Lines.Count - 1)
                {
                    // next credit
                    lineIndex++;
                    positionY = Game1.GAME_Y_RESOLUTION + 100;
                    pauseTimer = 0f;
                }
                else
                {
                    // The last one will just stop at the min
                    positionY = minY;

                    if (!creditsAreDone)
                    {
                        CreditsAreDone();
                        creditsAreDone = true;
                    }
                }
            }
        }

        public static void CreditsAreDone()
        {
            // Any achievements?
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            var creditLines = Lines[lineIndex];
            float scale = 1f;

            var lineHeight = Game1.Font.MeasureString(creditLines[0]).Y;
            var totalHeight = lineHeight * creditLines.Count;

            var yOffset = -totalHeight / 2;

            foreach (var line in creditLines)
            {
                var size = Game1.Font.MeasureString(line);
                
                spriteBatch.DrawString(Game1.Font, line, new Vector2(positionX, positionY + yOffset), Game1.SoftWhite, 0f, new Vector2(size.X / 2f, size.Y / 2f), scale, SpriteEffects.None, 0);
            
                yOffset += lineHeight;

            }


            
        }
    }
}
