using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SolTimer;

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

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            LoadHistory();
        }

        private void InitializeTimer()
        {
            timerService = TimerService.Instance;
            historyService = TimerHistoryService.Instance;
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Title.Text)) return;
            historyService.SaveTimer(TextBox_Title.Text, timerService.CurrentTime);
            LoadHistory();
            ResetTimer();
            MessageBox.Show("Timer saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TimerDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (compactTimerWindow == null) compactTimerWindow = new CompactTimerWindow(this);
                compactTimerWindow.ShowCompactTimer();
                this.Hide();
            }
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
            SetRightText();
        }

        private void SetRightText()
        {
            if (timerService.IsRunning)
            {
                StartPauseButton.Content = "Pause";
            }
            else
            {
                StartPauseButton.Content = "Start";
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is TimerEntry entry)
            {
                var result = MessageBox.Show(
                                    $"Are you sure you want to delete the timer '{entry.Title}'?",
                                    "Delete Timer",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
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
    }
}