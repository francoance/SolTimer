using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using SolTimer.ViewModels;

namespace SolTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Code-behind is intentionally thin: window chrome, compact mode transitions,
    /// dialog host coordination, and theme toggling (which requires WPF UI context).
    /// All timer/history business logic lives in <see cref="MainViewModel"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompactTimerWindow? _compactTimerWindow;
        private readonly SettingsService _settingsService;
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            _settingsService = SettingsService.Instance;

            var vm = new MainViewModel();
            DataContext = vm;

            InitializeComponent();
            InitializeTheme();
        }

        // ------------------------------------------------------------------ //
        //  Window chrome
        // ------------------------------------------------------------------ //

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimerService.Instance.IsDirty)
            {
                var promptDialog = new PromptDialog
                {
                    Message = { Text = "Querés cerrar sin guardar?" },
                    Accept = { Content = "Salir" }
                };

                var result = (await DialogHost.Show(promptDialog, "MainWindowDialog")) as bool?;
                if (result == null || !result.Value)
                    return;
            }
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _compactTimerWindow?.Close();
        }

        // ------------------------------------------------------------------ //
        //  Compact mode
        // ------------------------------------------------------------------ //

        private void LaunchCompactMode_MouseClick(object sender, RoutedEventArgs e)
            => LaunchCompactMode();

        private void LaunchCompactMode()
        {
            _compactTimerWindow ??= new CompactTimerWindow(this);
            _compactTimerWindow.ShowCompactTimer();
            Hide();
        }

        /// <summary>Called by <see cref="CompactTimerWindow"/> when returning to full view.</summary>
        public void ShowMainWindow()
        {
            Show();
        }

        private void TimerDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                LaunchCompactMode();
        }

        // ------------------------------------------------------------------ //
        //  Theme toggle (needs PaletteHelper — UI-layer concern)
        // ------------------------------------------------------------------ //

        private void InitializeTheme()
        {
            var darkTheme = _settingsService.GetSettings().DarkTheme;
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
            theme.SetBaseTheme(ThemeToggle.IsChecked == true ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            _settingsService.GetSettings().DarkTheme = ThemeToggle.IsChecked == true;
            _settingsService.SaveSettings();
        }

        // ------------------------------------------------------------------ //
        //  Buttons that require a confirmation dialog (UI concern)
        // ------------------------------------------------------------------ //

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var promptDialog = new PromptDialog
            {
                Message = { Text = "Reiniciar timer?" },
                Accept = { Content = "Aceptar" }
            };

            var result = (await DialogHost.Show(promptDialog, "MainWindowDialog")) as bool?;
            if (result != null && result.Value)
                ViewModel.Reset();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveCommand.Execute(null);
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
                if (result != null && result.Value)
                    ViewModel.DeleteEntry(entry);
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
                    MessageBox.Show($"Could not open URL: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No URL associated with this timer.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ------------------------------------------------------------------ //
        //  Keyboard shortcuts
        // ------------------------------------------------------------------ //

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ignore shortcuts when the user is typing in the title text box
            if (TextBox_Title.IsFocused)
                return;

            switch (e.Key)
            {
                case Key.Space:
                    ViewModel.StartPauseCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.S when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                    ViewModel.SaveCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.R when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                    ResetButton_Click(sender, e);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    LaunchCompactMode();
                    e.Handled = true;
                    break;
            }
        }
    }
}
