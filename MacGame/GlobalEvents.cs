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
        public static event EventHandler? SockCollected;

        /// <summary>
        /// A door is starting to open/close on Mac. This event notifieds the game state to stop 
        /// updating Mac and enemies and such. When the door closes it will raise DoorEntered to actually transition
        /// the player to the new map. 
        /// </summary>
        public static event EventHandler? BeginDoorEnter;

        /// <summary>
        /// You successfully entered a door and are transitioning to a new map.
        /// </summary>
        public static event EventHandler<DoorEnteredEventArgs>? DoorEntered;

        public static event EventHandler? OneHundredTacosCollected;

        public static event EventHandler? IntroComplete;

        public static void FireSockCollected(Object sender, EventArgs args)
        {
            var evt = SockCollected;
            if (evt != null)
            {
                evt(sender, args);
            }
        }

        public static void FireBeginDoorEnter(Object sender, EventArgs args)
        {
            var evt = BeginDoorEnter;
            if (evt != null)
            {
                evt(sender, args);
            }
        }

        public static void FireDoorEntered(Object sender, string transitionToMap, string putPlayerAtDoor, string doorNameEntered)
        {
            var evt = DoorEntered;
            if (evt != null)
            {
                var args = new DoorEnteredEventArgs(transitionToMap, putPlayerAtDoor, doorNameEntered);
                evt(sender, args);
            }
        }

        public static void FireIntroComplete()
        {
            var evt = IntroComplete;
            if (evt != null)
            {
                evt(null, EventArgs.Empty);
            }
        }

    }

    public class  DoorEnteredEventArgs : EventArgs
    {
        public string TransitionToMap { get; set; }
        public string PutPlayerAtDoor { get; set; }
        public string DoorNameEntered { get; set; }

        public DoorEnteredEventArgs(string transitionToMap, string putPlayerAtDoor, string doorNameEntered)
        {
            TransitionToMap = transitionToMap;
            PutPlayerAtDoor = putPlayerAtDoor;
            DoorNameEntered = doorNameEntered;
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