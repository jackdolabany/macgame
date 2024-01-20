using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacGame
{
    /// <summary>
    /// Manages crazy ass timers
    /// </summary>
    public static class TimerManager
    {
        private static List<Timer> timers = new List<Timer>(20);
        private static Queue<Timer> timersToAdd = new Queue<Timer>(10);

        public static void Clear()
        {
            timers.Clear();
        }

        public static void Update(float elapsed)
        {
            foreach (var timer in timers)
            {
                timer.Update(elapsed);
            }
            // Add the timers to add after the colleciton is enumerated. This way timers can be added by timer methods.
            while (timersToAdd.Count > 0)
            {
                var timer = timersToAdd.Dequeue();
                timers.Add(timer);
            }
        }

        public static Timer Then(this Timer timer, System.Action onTimer)
        {
            var nextTimer = new Timer(0, 0, onTimer, 0);
            timer.AddEnableAfterTimer(nextTimer);
            return AddTimer(nextTimer);
        }

        public static Timer Then(this Timer timer, float from, float to, System.Action onTimer, int cycles)
        {
            var nextTimer = new Timer(from, to, onTimer, cycles);
            timer.AddEnableAfterTimer(nextTimer);
            return AddTimer(nextTimer);
        }

        public static Timer Then(this Timer timer, float from, float to, System.Action onTimer, bool repeat)
        {
            var nextTimer = new Timer(from, to, onTimer, repeat ? int.MaxValue : 0);
            timer.AddEnableAfterTimer(nextTimer);
            return AddTimer(nextTimer);
        }

        public static Timer Then(this Timer timer, float time, System.Action onTimer, bool repeat)
        {
            var nextTimer = new Timer(time, time, onTimer, repeat ? int.MaxValue : 0);
            timer.AddEnableAfterTimer(nextTimer);
            return AddTimer(nextTimer);
        }

        public static Timer AddTimer(Timer timer)
        {
            timersToAdd.Enqueue(timer);
            return timer;
        }

        public static Timer AddNewTimer(float from, float to, System.Action action, int cycles = int.MaxValue)
        {
            var timer = new Timer(from, to, action, cycles);
            return AddTimer(timer);
        }

        public static Timer AddNewTimer(float time, System.Action action, bool repeat)
        {
            return AddNewTimer(time, time, action, repeat ? int.MaxValue : 0);
        }

        public static Timer AddNewTimer(float time, System.Action action, int cycles = int.MaxValue)
        {
            return AddNewTimer(time, time, action, cycles);
        }

    }
}
