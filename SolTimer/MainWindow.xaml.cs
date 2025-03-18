using System.Windows;
using System.Windows.Input;

namespace SolTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompactTimerWindow compactTimerWindow;
        private TimerService timerService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
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

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (timerService.IsRunning)
            {
                timerService.Pause();
                StartPauseButton.Content = "Start";
            }
            else
            {
                timerService.Start();
                StartPauseButton.Content = "Pause";
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            timerService.Reset();
            StartPauseButton.Content = "Start";
        }

        private void TimerDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (compactTimerWindow == null || !compactTimerWindow.IsVisible)
                {
                    compactTimerWindow = new CompactTimerWindow(this);
                    compactTimerWindow.Show();
                }
                this.Hide();
            }
        }

        public void ShowMainWindow()
        {
            this.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (compactTimerWindow != null)
            {
                compactTimerWindow.Close();
            }
        }
    }
}