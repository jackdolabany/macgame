using System;
using System.Globalization;

namespace MacGame
{
    /// <summary>
    /// You can fire events that make major changes to game state from here. These events help decouple classes that shouldn't know
    /// about each other. Usually the game instance and items or doors or something.
    /// </summary>
    public static class GlobalEvents
    {
        public static event EventHandler? CricketCoinCollected;
        public static event EventHandler<DoorEnteredEventArgs>? DoorEntered;
        public static event EventHandler<SubWorldDoorEnteredEventArgs>? SubWorldDoorEntered;
        public static event EventHandler? OneHundredTacosCollected;

        public static void FireCricketCoinCollected(Object sender, EventArgs args)
        {
            var evt = CricketCoinCollected;
            if (evt != null)
            {
                evt(sender, args);
            }
        }

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

        public static void FireOneHundredTacosCollected(Object sender, EventArgs args)
        {
            var evt = OneHundredTacosCollected;
            if (evt != null)
            {
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