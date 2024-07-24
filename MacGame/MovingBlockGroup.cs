using Microsoft.Xna.Framework;

namespace MacGame
{
    /// <summary>
    /// This represents a group of blocks that can move up or down. Like floating platforms that change when you adjust the water levels. 
    /// Or maybe blocks will just move around in the ghost house or something.
    /// </summary>
    public class MovingBlockGroup
    {
        public string Name { get; set; }

        /// <summary>
        /// The values of the rectangle are the x/y tile coordinates of the block. Not the
        /// float locations.
        /// </summary>
        public Rectangle Rectangle { get; set; }

        // High water offset assumed to be 0/default.

        public int LowWaterOffset { get; set; }
        public int MediumWaterOffset { get; set; }
    }
}
