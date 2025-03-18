using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

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
            UpdatePauseButton();
        }

        private void InitializeTimer()
        {
            timerService = TimerService.Instance;
            timerService.OnTimeUpdated += TimerService_OnTimeUpdated;
            UpdateDisplay();
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
            StatusIcon.Kind = PackIconJamIconsKind.Play;
        }

        private void SetPauseButton()
        {
            StatusIcon.Kind = PackIconJamIconsKind.Pause;
        }
    }
} 