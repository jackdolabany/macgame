using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// Handles background effects like rain, snow, or stars in space.
    /// </summary>
    public class BackgroundEffectsManager
    {

        private List<Star> _stars = new List<Star>();

        private bool _isShowingStars = false;
        private bool _isInitialized = false;

        public BackgroundEffectsManager()
        {
            // Create some stars.
            for (int i = 0; i < 100; i++)
            {
                _stars.Add(new Star());
            }
        }

        /// <summary>
        /// Call this every time a new map loads.
        /// </summary>
        public void Reset()
        {
            _isShowingStars = false;
            _isInitialized = false;
        }

        public void ShowStars()
        {
            _isShowingStars = true;
        }

        public void Initialize()
        {
            // Initialize all the background effects.
            _isInitialized = true;


            foreach (var star in _stars)
            {
                InitializeStar(star);

                // Set the x coordinate to a random value on screen to start.
                var randomX = Game1.Randy.Next(Game1.Camera.ViewPort.X, Game1.Camera.ViewPort.Right);
                star.Position = new Vector2(randomX, star.Position.Y);
            }
        }

        private void InitializeStar(Star star)
        {
            var randomHeight = Game1.Randy.Next(0, Game1.CurrentLevel.Map.MapHeight * Game1.TileSize * Game1.TileScale);
            star.Position = new Vector2(star.Position.X, randomHeight);

            // Give it a random brightness between 0.1 and 1.0.
            star.Transparency = Game1.Randy.Next(1, 9) / 10.0f;

            // Give it a random velocity but moving to the left
            
            star.Velocity = new Vector2(-Game1.Randy.Next(5, 11) * 20 , 0);

        }

        public void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_isShowingStars)
            {
                foreach (var star in _stars)
                {
                    star.Update(elapsed, gameTime);

                    // Send the star back if it went off the screen.
                    if (star.Position.X < Game1.Camera.ViewPort.X - 16)
                    {
                        InitializeStar(star);
                        star.Position = new Vector2(Game1.Camera.ViewPort.Right, star.Position.Y);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isShowingStars)
            {
                foreach (var star in _stars)
                {
                    star.Draw(spriteBatch);
                }
            }
        }
    }
}
