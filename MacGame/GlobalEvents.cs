using System;

namespace MacGame
{
    public static class GlobalEvents
    {
        public static event EventHandler? CricketCoinCollected;

        public static void FireCricketCoinCollected(Object sender, EventArgs args)
        {
            var evt = CricketCoinCollected;
            if (evt != null)
            {
                evt(sender, args);
            }
        }
    }
}