using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// If socks are in the TileMap they need an object square thing around them that gives them a name. This will give them a
    /// hint and act as an index to get the total sock count.
    /// </summary>
    public static class SockIndex
    {
        public static Dictionary<int, List<SockInfo>> LevelNumberToSocks = new Dictionary<int, List<SockInfo>>
        {
            { 
                1, 
                new List<SockInfo>
                {
                    new SockInfo { Name = "RoofSock", Hint = "There's a sock stuck on my buddy's roof." },
                    new SockInfo { Name = "TreeCannonSock", Hint = "There's a stronger than normal cannon between the trees." },
                    new SockInfo { Name = "TreeSock", Hint = "Looking for adventure? Try to climb all the way up the tree." },
                    new SockInfo { Name = "CannonSock", Hint = "Up, left, up, up, left, up, up, boom" },
                    new SockInfo { Name = "SandSock", Hint = "There's a legend about a lonely traffic cone" },
                    new SockInfo { Name = "IceSock", Hint = "There's something special if you can trek through icy mountains." },
                    new SockInfo { Name = "SandTopSock", Hint = "Deep in the caves is a room with so much sand, it'll make you puke." },
                    new SockInfo { Name = "MinecartSock", Hint = "I heard there's still something shiny down in the old mine." },
                    new SockInfo { Name = "TacoSock", Hint = "The hungry mouse." },
                    new SockInfo { Name = "RaceSock", Hint = "Boy that Froggy sure is fast! And annoying." },
                }
            },
            {
                2, 
                new List<SockInfo>
                {
                    new SockInfo { Name = "Sock1", Hint = "I'm over to the left" },
                    new SockInfo { Name = "Sock2", Hint = "The sock to the right" },
                    new SockInfo { Name = "Sock3", Hint = "A cheese sock. It smells like cheese." },
                    new SockInfo { Name = "TacoSock", Hint = "Next to the vine" },
                }
            },
        };
    }

    public class WorldSocks
    {
        public int WorldNumber { get; set; }
        public List<SockInfo> Socks { get; set; }
    }

    public class SockInfo
    {
        public string Name { get; set; }
        public string Hint { get; set; }
    }
}
