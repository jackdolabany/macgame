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
                    new SockInfo { Name = "WaterMazeSock", Hint = "There's a terrible maze deep in the water somewhere. I heard there's a sock stashed at the end." },
                    new SockInfo { Name = "RedDoorSock", Hint = "Find the red key." },
                    new SockInfo { Name = "GreenDoorSock", Hint = "Find the green key." },
                    new SockInfo { Name = "BlueDoorSock", Hint = "Find the blue key." },
                    new SockInfo { Name = "TacoSock", Hint = "Talk to that taco loving rat." },
                    new SockInfo { Name = "SpringboardSock", Hint = "I heard those crazy cats trapped a frog. Did you see it?" },
                    new SockInfo { Name = "GooseBossSock", Hint = "Roast the goose!" },
                    new SockInfo { Name = "MinecartSock", Hint = "If you're feeling stressed take a nice relaxing minecart ride." },
                    new SockInfo { Name = "BlowfishBossSock", Hint = "I heard a mad puffer fish is trying to start a sock collection. They don't even have feet." },
                    new SockInfo { Name = "FlappySock", Hint = "Deep in a cave nearby is a magical door to magical land where you can fly like a fish." },
                    new SockInfo { Name = "RaceSock", Hint = "If you're having trouble beating the frog in a race, just cheat. He won't notice." },
                }
            },{
                3,
                new List<SockInfo>
                {
                    new SockInfo { Name = "DraculaSock", Hint = "To resurrect Dracula you must find his 3 remains." },
                    new SockInfo { Name = "QuadcopterSock", Hint = "Fight the drone wars you must." },
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
