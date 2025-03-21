using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace SolTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompactTimerWindow compactTimerWindow;
        private TimerService timerService;
        private TimerHistoryService historyService;
        private SettingsService settingsService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDependencies();
            InitializeTimer();
            LoadHistory();
            InitializeTheme();
        }

        private void InitializeDependencies()
        {
            timerService = TimerService.Instance;
            historyService = TimerHistoryService.Instance;
            settingsService = SettingsService.Instance;

            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolTimer");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
        }

        private void InitializeTimer()
        {
            timerService.OnTimeUpdated += TimerService_OnTimeUpdated;
            UpdateDisplay();
        }

        private void LoadHistory()
        {
            var history = historyService.GetHistory();
            HistoryListView.ItemsSource = history;
        }

        private void TimerService_OnTimeUpdated(object sender, TimeSpan time)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TimerDisplay.Text = timerService.CurrentTime.ToString(@"hh\:mm\:ss");
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (timerService.IsRunning)
            {
                timerService.Pause();
                SetRightText();
            }
            else
            {
                timerService.Start();
                SetRightText();
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var promptDialog = new PromptDialog
            {
                Message = { Text = $"Reiniciar timer?" },
                Accept = { Content = "Aceptar" }
            };

            var result = (await DialogHost.Show(promptDialog, "MainWindowDialog")) as bool?;
            if (result != null || result.Value)
            {
                ResetTimer();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Title.Text)) return;
            historyService.SaveTimer(TextBox_Title.Text, timerService.CurrentTime);
            LoadHistory();
            ResetTimer();
            // Al pedo abrir el save
            //var sampleMessageDialog = new SavedTimerDialog
            //{
            //    Message = { Text = "Timer saved successfully!" }
            //};

            //await DialogHost.Show(sampleMessageDialog, "MainWindowDialog");
        }

        private void TimerDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                LaunchCompactMode();
            }
        }

        private void LaunchCompactMode_MouseClick(object sender, RoutedEventArgs e)
        {
            LaunchCompactMode();
        }

        private void LaunchCompactMode()
        {
            if (compactTimerWindow == null) compactTimerWindow = new CompactTimerWindow(this);
            compactTimerWindow.ShowCompactTimer();
            this.Hide();
        }

        public void ShowMainWindow()
        {
            this.Show();
            SetRightText();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (compactTimerWindow != null)
            {
                compactTimerWindow.Close();
            }
        }

        private void ResetTimer()
        {
            timerService.Reset();
            TextBox_Title.Clear();
            SetRightText();
        }

        private void SetRightText()
        {
            if (timerService.IsRunning)
            {
                StartPauseButton.Content = "Pausar";
            }
            else
            {
                StartPauseButton.Content = "Arrancar";
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is TimerEntry entry)
            {
                var promptDialog = new PromptDialog
                {
                    Message = { Text = $"Borrar {entry.Title}?" },
                    Accept = { Content = "Aceptar" }
                };

                var result = (await DialogHost.Show(promptDialog, "MainWindowDialog")) as bool?;
                if (result != null || result.Value)
                {
                    historyService.DeleteTimer(entry);
                    LoadHistory();
                }
            }
        }

        private void UrlButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is TimerEntry entry)
            {
                var url = $"https://airteam.cloud/project/{entry.Title}";
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No URL associated with this timer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (timerService.IsDirty)
            {
                var promptDialog = new PromptDialog
                {
                    Message = { Text = "Querés cerrar sin guardar?" },
                    Accept = { Content = "Salir" }
                };

                var result = (await DialogHost.Show(promptDialog, "MainWindowDialog")) as bool?;               
                if (result == null || !result.Value)
                {
                    return;
                }
            }
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void InitializeTheme()
        {
            // Set initial theme state
            var darkTheme = settingsService.GetSettings().DarkTheme;
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(darkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            ThemeToggle.IsChecked = darkTheme;
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(ThemeToggle.IsChecked == true ? 
                BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            settingsService.GetSettings().DarkTheme = ThemeToggle.IsChecked.Value;
            settingsService.SaveSettings();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is TimerEntry entry)
            {
                timerService.SetTime(entry.Duration);
                TextBox_Title.Text = entry.Title;
                historyService.DeleteTimer(entry);
                LoadHistory();
                timerService.Start();
                SetRightText();
            }
        }
    }
}