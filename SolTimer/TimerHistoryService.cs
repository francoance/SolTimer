using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace SolTimer
{
    public class TimerHistoryService
    {
        private static TimerHistoryService instance;
        private List<TimerEntry> timerHistory;
        private readonly string historyFilePath;

        public static TimerHistoryService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimerHistoryService();
                }
                return instance;
            }
        }

        private TimerHistoryService()
        {
            historyFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolTimer",
                "timer_history.json");
            LoadHistory();
        }

        public List<TimerEntry> GetHistory()
        {
            return timerHistory.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public void SaveTimer(string title, TimeSpan duration)
        {
            var entry = new TimerEntry(title, duration);
            timerHistory.Add(entry);
            SaveHistory();
        }

        public void DeleteTimer(TimerEntry entry)
        {
            timerHistory.Remove(entry);
            SaveHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(historyFilePath))
                {
                    var json = File.ReadAllText(historyFilePath);
                    timerHistory = JsonSerializer.Deserialize<List<TimerEntry>>(json);
                }
                else
                {
                    timerHistory = new List<TimerEntry>();
                }
            }
            catch
            {
                timerHistory = new List<TimerEntry>();
            }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(timerHistory);
                File.WriteAllText(historyFilePath, json);
            }
            catch
            {
                // Handle save error silently
            }
        }
    }
} 