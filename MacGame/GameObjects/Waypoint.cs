using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// Waypoints are locations for certain characters to follow.
    /// </summary>
    public class Waypoint
    {
        public Waypoint(int x, int y)
        {
            Location = new Vector2(x * Game1.TileSize + Game1.TileSize / 2, y * Game1.TileSize + Game1.TileSize / 2);
        }

        /// <summary>
        /// The center of the waypoint.
        /// </summary>
        public Vector2 Location { get; set; }
    }

    public class  Waypath
    {
        /// <summary>
        /// This is expected to be ordered at the time it is instantiated.
        /// </summary>
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
    }
}
