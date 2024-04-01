using System;
using System.Globalization;

namespace MacGame
{
    public static class GlobalEvents
    {
        public static event EventHandler? CricketCoinCollected;
        public static event EventHandler<DoorEnteredEventArgs>? DoorEntered;
        public static event EventHandler<SubWorldDoorEnteredEventArgs>? SubWorldDoorEntered;

        public static void FireCricketCoinCollected(Object sender, EventArgs args)
        {
            var evt = CricketCoinCollected;
            if (evt != null)
            {
                evt(sender, args);
            }
        }

        public static string DoorJustEntered { get; internal set; }

        // If a player goes through a door here's the map you're going to.
        public static string TransitionToMap;

        // If you enter a door connected to another door, this is the door you are going to.
        public static string PutPlayerAtDoor;

        public static bool IsTransitionToSubWorld;

        public static void FireDoorEntered(Object sender, string transitionToMap, string putPlayerAtDoor, string doorNameEntered, int? newHintIndex = null)
        {
            var evt = DoorEntered;
            if (evt != null)
            {
                var args = new DoorEnteredEventArgs(transitionToMap, putPlayerAtDoor, doorNameEntered, newHintIndex);
                evt(sender, args);
            }
        }

        public static void FireSubWorldDoorEntered(Object sender, string doorNameEntered, string transitionToMap)
        {
            var evt = SubWorldDoorEntered;
            if (evt != null)
            {
                var args = new SubWorldDoorEnteredEventArgs(doorNameEntered, transitionToMap);
                evt(sender, args);
            }
        }
    }

    public class  DoorEnteredEventArgs : EventArgs
    {
        public string TransitionToMap { get; set; }
        public string PutPlayerAtDoor { get; set; }
        public string DoorNameEntered { get; set; }
        public int? NewHintIndex { get; set; }

        public DoorEnteredEventArgs(string transitionToMap, string putPlayerAtDoor, string doorNameEntered, int? newHintIndex)
        {
            TransitionToMap = transitionToMap;
            PutPlayerAtDoor = putPlayerAtDoor;
            DoorNameEntered = doorNameEntered;
            NewHintIndex = newHintIndex;
        }
    }

    public class SubWorldDoorEnteredEventArgs : EventArgs
    {
        public string DoorNameEntered { get; set; }
        public string TransitionToMap { get; set; }

        public SubWorldDoorEnteredEventArgs(string doorNameEntered, string transitionToMap)
        {
            DoorNameEntered = doorNameEntered;
            TransitionToMap = transitionToMap;
        }
    }
}