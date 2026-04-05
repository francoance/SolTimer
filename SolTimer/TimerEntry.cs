using System;

namespace SolTimer
{
    public class TimerEntry
    {
        /// <summary>SQLite row identity; 0 for entries not yet persisted.</summary>
        public int Id { get; set; }

        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DayGroup => CreatedAt.ToString("dd/MM/yyyy");

        /// <summary>Creates a new entry stamped with the current time.</summary>
        public TimerEntry(string title, TimeSpan duration)
        {
            Title = title;
            Duration = duration;
            CreatedAt = DateTime.Now;
        }

        /// <summary>Parameterless constructor required for JSON deserialization during migration.</summary>
        public TimerEntry()
        {
            Title = string.Empty;
        }
    }
}
