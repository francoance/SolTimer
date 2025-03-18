using System;
using System.Windows.Threading;

namespace SolTimer
{
    public class TimerService
    {
        private static TimerService instance;
        private DispatcherTimer timer;
        private TimeSpan time;
        private bool isRunning;

        public static TimerService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimerService();
                }
                return instance;
            }
        }

        private TimerService()
        {
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            time = TimeSpan.Zero;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            time = time.Add(TimeSpan.FromSeconds(1));
            OnTimeUpdated?.Invoke(this, time);
        }

        public event EventHandler<TimeSpan> OnTimeUpdated;

        public void Start()
        {
            timer.Start();
            isRunning = true;
        }

        public void Pause()
        {
            timer.Stop();
            isRunning = false;
        }

        public void Reset()
        {
            timer.Stop();
            time = TimeSpan.Zero;
            OnTimeUpdated?.Invoke(this, time);
            isRunning = false;
        }

        public bool IsRunning => isRunning;
        public TimeSpan CurrentTime => time;
    }
} 