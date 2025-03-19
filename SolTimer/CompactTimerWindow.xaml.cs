using System.Windows;
using System.Windows.Input;
using MahApps.Metro.IconPacks;
using MaterialDesignThemes.Wpf;

namespace SolTimer
{
    public partial class CompactTimerWindow : Window
    {
        private TimerService timerService;
        private MainWindow mainWindow;

        public CompactTimerWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            InitializeTimer();
            LoadPosition();
            UpdatePauseButton();
        }

        public void ShowCompactTimer()
        {
            this.Show();
            UpdatePauseButton();
            LoadPosition();
        }

        private void InitializeTimer()
        {
            timerService = TimerService.Instance;
            timerService.OnTimeUpdated += TimerService_OnTimeUpdated;
            UpdateDisplay();
        }

        private void LoadPosition()
        {
            var settings = SettingsService.Instance.GetSettings();
            Left = settings.CompactPosX.Value;
            Top = settings.CompactPosY.Value;
        }

        private void SavePosition()
        {
            var settings = SettingsService.Instance.GetSettings();
            settings.CompactPosX = Left;
            settings.CompactPosY = Top;
            SettingsService.Instance.SaveSettings();
        }

        private void TimerService_OnTimeUpdated(object sender, TimeSpan time)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TimerDisplay.Text = timerService.CurrentTime.ToString(@"hh\:mm\:ss");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
                SavePosition();
            }
        }

        private void TimerDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.Hide();
                mainWindow.ShowMainWindow();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (timerService.IsRunning)
            {
                timerService.Pause();
                SetPlayButton();
            }
            else
            {
                timerService.Start();
                SetPauseButton();
            }
        }

        private void UpdatePauseButton()
        {
            if (timerService.IsRunning)
            {
                SetPauseButton();
            } else
            {
                SetPlayButton();
            }
        }

        private void SetPlayButton()
        {
            StatusIcon.Kind = PackIconKind.Play;
        }

        private void SetPauseButton()
        {
            StatusIcon.Kind = PackIconKind.Pause;
        }
    }
} 