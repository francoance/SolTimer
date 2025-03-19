
using System.IO;
using System.Text.Json;

namespace SolTimer
{
    public class SettingsService
    {
        private static SettingsService instance;
        private readonly string settingsFilePath;
        private Settings settings;

        public static SettingsService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsService();
                }
                return instance;
            }
        }

        private SettingsService()
        {
            settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolTimer",
                "settings.json");
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    var json = File.ReadAllText(settingsFilePath);
                    settings = JsonSerializer.Deserialize<Settings>(json);
                }
                else
                {
                    settings = new Settings();
                }
            }
            catch
            {
                settings = new Settings();
            }

            if (settings.CompactPosX is null) settings.CompactPosX = 0;
            if (settings.CompactPosY is null) settings.CompactPosY = 0;
        }

        public Settings GetSettings()
        {
            return settings;
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(settingsFilePath, json); 
            }
            catch
            {
                // Handle save error silently
            }
        }
    }
}
