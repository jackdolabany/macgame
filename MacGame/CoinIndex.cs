using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// If coins are in the TileMap they need an object square thing around them that gives them a name. This will give them a
    /// hint and act as an index to get the total coin count.
    /// </summary>
    public static class CoinIndex
    {
        public static Dictionary<int, List<CoinInfo>> LevelNumberToCoins = new Dictionary<int, List<CoinInfo>>
        {
            { 
                1, 
                new List<CoinInfo>
                {
                    new CoinInfo { Name = "RoofCoin", Hint = "There's a coin stuck on my buddy's roof." },
                    new CoinInfo { Name = "TreeCannonCoin", Hint = "There's a stronger than normal cannon between the trees." },
                    new CoinInfo { Name = "TreeCoin", Hint = "Looking for adventure? Try to climb all the way up the tree." },
                    new CoinInfo { Name = "CannonCoin", Hint = "Up, left, up, up, left, up, up, boom" },
                    new CoinInfo { Name = "SandCoin", Hint = "There's a legend about a lonely traffic cone" },
                    new CoinInfo { Name = "IceCoin", Hint = "There's something special if you can trek through icy mountains." },
                    new CoinInfo { Name = "MinecartCoin", Hint = "I heard there's still something shiny down in the old mine." },
                    new CoinInfo { Name = "TacoCoin", Hint = "The hungry mouse." },
                }
            },
            {
                2, 
                new List<CoinInfo>
                {
                    new CoinInfo { Name = "Coin1", Hint = "I'm over to the left" },
                    new CoinInfo { Name = "Coin2", Hint = "The coin to the right" },
                    new CoinInfo { Name = "Coin3", Hint = "A cheese coin. It smells like cheese." },
                    new CoinInfo { Name = "TacoCoin", Hint = "Next to the vine" },
                }
            },
        };
    }

    public class WorldCoins
    {
        public int WorldNumber { get; set; }
        public List<CoinInfo> Coins { get; set; }
    }

    public class CoinInfo
    {
        public string Name { get; set; }
        public string Hint { get; set; }
    }
}
