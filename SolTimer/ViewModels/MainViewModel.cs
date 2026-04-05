using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SolTimer.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="MainWindow"/>. Owns all timer-control and
    /// history-management logic; the view binds to its properties and commands.
    /// </summary>
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly TimerService _timerService;
        private readonly TimerHistoryService _historyService;

        private string _currentTimeDisplay = "00:00:00";
        private string _projectTitle = string.Empty;
        private bool _isRunning;

        // ------------------------------------------------------------------ //
        //  Construction
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Initialises a new instance of <see cref="MainViewModel"/> using
        /// the application-wide singleton services.
        /// </summary>
        public MainViewModel()
            : this(TimerService.Instance, TimerHistoryService.Instance)
        {
        }

        /// <summary>
        /// Initialises a new instance of <see cref="MainViewModel"/> with
        /// explicit service dependencies (for testing).
        /// </summary>
        public MainViewModel(TimerService timerService, TimerHistoryService historyService)
        {
            _timerService = timerService;
            _historyService = historyService;

            _timerService.OnTimeUpdated += OnTimeUpdated;

            History = new ObservableCollection<TimerEntry>(_historyService.GetHistory());

            StartPauseCommand = new RelayCommand(ExecuteStartPause);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            ResetCommand = new RelayCommand(ExecuteReset);
            DeleteEntryCommand = new RelayCommand(ExecuteDeleteEntry);
            ResumeEntryCommand = new RelayCommand(ExecuteResumeEntry);

            RefreshDisplay();
        }

        // ------------------------------------------------------------------ //
        //  Properties
        // ------------------------------------------------------------------ //

        /// <summary>Gets the formatted timer string in hh:mm:ss.</summary>
        public string CurrentTimeDisplay
        {
            get => _currentTimeDisplay;
            private set => SetField(ref _currentTimeDisplay, value);
        }

        /// <summary>Gets or sets the project title entered by the user.</summary>
        public string ProjectTitle
        {
            get => _projectTitle;
            set
            {
                if (SetField(ref _projectTitle, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>Gets whether the timer is currently running.</summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set => SetField(ref _isRunning, value);
        }

        /// <summary>Gets whether a save operation is currently possible.</summary>
        public bool CanSave => !string.IsNullOrEmpty(ProjectTitle);

        /// <summary>Gets the label for the start/pause toggle button.</summary>
        public string StartPauseLabel => IsRunning ? "Pausar" : "Arrancar";

        /// <summary>Gets the flat history collection; XAML groups it via CollectionViewSource.</summary>
        public ObservableCollection<TimerEntry> History { get; }

        // ------------------------------------------------------------------ //
        //  Commands
        // ------------------------------------------------------------------ //

        /// <summary>Starts or pauses the timer.</summary>
        public ICommand StartPauseCommand { get; }

        /// <summary>Saves the current timer entry; only executable when a title is present.</summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Resets the timer. The view is responsible for showing a confirmation
        /// dialog; it calls <see cref="Reset"/> after the user confirms.
        /// </summary>
        public ICommand ResetCommand { get; }

        /// <summary>
        /// Deletes a history entry. The view passes the <see cref="TimerEntry"/>
        /// as the command parameter after showing a confirmation dialog.
        /// </summary>
        public ICommand DeleteEntryCommand { get; }

        /// <summary>
        /// Resumes a history entry. The view passes the <see cref="TimerEntry"/>
        /// as the command parameter.
        /// </summary>
        public ICommand ResumeEntryCommand { get; }

        // ------------------------------------------------------------------ //
        //  Public imperative methods (called from code-behind after dialogs)
        // ------------------------------------------------------------------ //

        /// <summary>Resets the timer and clears the project title.</summary>
        public void Reset()
        {
            _timerService.Reset();
            ProjectTitle = string.Empty;
            RefreshRunningState();
        }

        /// <summary>Deletes the supplied history entry and refreshes the list.</summary>
        public void DeleteEntry(TimerEntry entry)
        {
            _historyService.DeleteTimer(entry);
            RefreshHistory();
        }

        // ------------------------------------------------------------------ //
        //  INotifyPropertyChanged
        // ------------------------------------------------------------------ //

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        // ------------------------------------------------------------------ //
        //  Private helpers
        // ------------------------------------------------------------------ //

        private void OnTimeUpdated(object? sender, TimeSpan time)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            CurrentTimeDisplay = _timerService.CurrentTime.ToString(@"hh\:mm\:ss");
        }

        private void RefreshRunningState()
        {
            IsRunning = _timerService.IsRunning;
            OnPropertyChanged(nameof(StartPauseLabel));
        }

        private void RefreshHistory()
        {
            History.Clear();
            foreach (var entry in _historyService.GetHistory())
                History.Add(entry);
        }

        private void ExecuteStartPause(object? _)
        {
            if (_timerService.IsRunning)
                _timerService.Pause();
            else
                _timerService.Start();

            RefreshRunningState();
        }

        private void ExecuteSave(object? _)
        {
            if (string.IsNullOrEmpty(ProjectTitle)) return;
            _historyService.SaveTimer(ProjectTitle, _timerService.CurrentTime);
            RefreshHistory();
            Reset();
        }

        private bool CanExecuteSave(object? _) => !string.IsNullOrEmpty(ProjectTitle);

        private void ExecuteReset(object? _)
        {
            // The view shows a confirmation dialog before calling Reset().
            // This command is wired so the view can also trigger it via keyboard
            // shortcut — the code-behind handles the dialog flow.
            Reset();
        }

        private void ExecuteDeleteEntry(object? parameter)
        {
            if (parameter is TimerEntry entry)
                DeleteEntry(entry);
        }

        private void ExecuteResumeEntry(object? parameter)
        {
            if (parameter is not TimerEntry entry) return;

            _timerService.SetTime(entry.Duration);
            ProjectTitle = entry.Title;
            _historyService.DeleteTimer(entry);
            RefreshHistory();
            _timerService.Start();
            RefreshRunningState();
        }
    }
}
