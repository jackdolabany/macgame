using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    public enum WaterType : byte
    {

        NotWater = 0,

        /// <summary>
        /// This tile should animate like a wave.
        /// </summary>
        AnimatingTopOfWater = 1,

        /// <summary>
        /// Same as 1 but an alt color for darker background maps.
        /// </summary>
        AltAnimatingTopOfWater = 2,

        /// <summary>
        /// This tile acts like water but just renders as the tile it is.
        /// </summary>
        RegularWaterTile = 3,
    }
}
