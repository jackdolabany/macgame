using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.DisappearBlocks
{
    public class DisappearBlockGroup
    {
        public List<DisappearBlock> DisappearBlocks;

        /// <summary>
        /// A rectangle that overlaps the entire group that we can check
        /// before we need to do a more complex block by block collision test
        /// </summary>
        public Rectangle CollisionRectangle;
        public float CollisionTime;

        public string GroupName = "";
        public int nextSeriesToShow = 1;
        public int maxSeries = 1;

        // Be disabled for a time when you first come in screen.
        private float disabledTimer = 0f;
        private const float disabledTimerGoal = 3.0f;

        private const float TotalRevealTimerGoal = 3.0f;
        private const float ShowNextSeriesTimerGoal = 1.5f;
        private float showNextSeriesTimer = 0f;

        private bool enabled = false;

        public DisappearBlockGroup(string groupName)
        {
            GroupName = groupName;
            DisappearBlocks = new List<DisappearBlock>();
        }

        public void BuildStats()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = 0;
            int maxY = 0;

            foreach (var block in DisappearBlocks)
            {
                minX = Math.Min(minX, block.CellX);
                minY = Math.Min(minY, block.CellY);
                maxX = Math.Max(maxX, block.CellX);
                maxY = Math.Max(maxY, block.CellY);
                maxSeries = Math.Max(maxSeries, block.Series);
            }

            int tileWidth = TileEngine.TileMap.TileSize;
            int tileHeight = TileEngine.TileMap.TileSize;

            CollisionRectangle = new Rectangle(minX * tileWidth, minY * tileHeight, (maxX - minX + 1) * tileWidth, (maxY - minY + 1) * tileHeight);
        }

        public bool IsColliding(Rectangle rectangleToTest)
        {
            if (rectangleToTest.Intersects(CollisionRectangle))
            {
                foreach (var block in DisappearBlocks)
                {
                    if (block.IsColliding(rectangleToTest))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            var isOnScreen = Game1.Camera.IsObjectVisible(this.CollisionRectangle);

            if (enabled && !isOnScreen)
            {
                // Reset when we go off screen.
                enabled = false;
                disabledTimer = 0f;
                nextSeriesToShow = 1;
                foreach (var block in DisappearBlocks)
                {
                    block.Disappear();
                }
            }

            if (!enabled && isOnScreen)
            {
                disabledTimer += elapsed;
                if (disabledTimer > disabledTimerGoal)
                {
                    // Enable the blocks
                    enabled = true;
                    foreach (var block in DisappearBlocks)
                    {
                        block.Disappear();
                    }
                }
            }

            if (enabled && isOnScreen)
            {
                showNextSeriesTimer += elapsed;
                if (showNextSeriesTimer > ShowNextSeriesTimerGoal)
                {
                    showNextSeriesTimer = 0f;

                    // Show the next series
                    foreach (var block in DisappearBlocks)
                    {
                        if (block.Series == nextSeriesToShow)
                        {
                            SoundManager.PlaySound("BlockAppear", 0.3f, -0.2f);
                            block.Appear(TotalRevealTimerGoal);
                        }
                    }

                    // Increment the series to show
                    nextSeriesToShow++;
                    if (nextSeriesToShow > maxSeries)
                    {
                        nextSeriesToShow = 1;
                    }

                }

                foreach (var block in DisappearBlocks)
                {
                    block.Update(gameTime, elapsed);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach( var block in DisappearBlocks)
            {
                block.Draw(spriteBatch);
            }
        }
    }
}
