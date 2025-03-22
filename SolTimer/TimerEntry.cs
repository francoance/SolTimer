using System;
using System.Linq;

namespace SolTimer
{
    public class TimerEntry
    {
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DayGroup => CreatedAt.ToString("dd/MM/yyyy");

        public TimerEntry(string title, TimeSpan duration)
        {
            Title = title;
            Duration = duration;
            CreatedAt = DateTime.Now;
        }
    }
} 