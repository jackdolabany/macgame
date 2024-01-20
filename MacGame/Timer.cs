using MacGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacGame
{

    /// <summary>
    /// Use these timers for time bound effects. For example when you get hit you are invincible for 1s
    /// </summary>
    public class Timer
    {
        private float From { get; set; }
        private float To { get; set; }
        public System.Action OnTimer;
        private float TimeRemaining { get; set; }
        public bool Enabled { get; set; }
        private int Cycles { get; set; }
        private List<Timer> EnableAfter = new List<Timer>();

        public Timer(float from, float to, System.Action onTimer, bool repeat)
        {
            this.From = from;
            this.To = to;
            this.OnTimer = onTimer;
            this.Cycles = repeat ? int.MaxValue : 0;
            this.Enabled = true;
            Reset();
        }

        public Timer(float from, float to, System.Action onTimer, int cycles)
        {
            this.From = from;
            this.To = to;
            this.OnTimer = onTimer;
            this.Cycles = cycles;
            this.Enabled = true;
            Reset();
        }

        public void Reset()
        {
            float difference = To - From;
            TimeRemaining = From + ((float)Game1.Randy.NextDouble() * difference);
        }

        public void AddEnableAfterTimer(Timer timer)
        {
            timer.Enabled = false;
            this.EnableAfter.Add(timer);
        }

        public void Update(float elapsed)
        {
            if (!Enabled) return;
            TimeRemaining -= elapsed;
            if (TimeRemaining <= 0)
            {
                OnTimer.Invoke();
                if (Cycles > 1)
                {
                    Cycles--;
                    Reset();
                }
                else
                {
                    this.Enabled = false;
                    foreach (var timer in EnableAfter)
                    {
                        timer.Enabled = true;
                    }
                }
            }
        }
    }
}
