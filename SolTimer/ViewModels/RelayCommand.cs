using System;
using System.Windows.Input;

namespace SolTimer.ViewModels
{
    /// <summary>
    /// A generic <see cref="ICommand"/> implementation that delegates execution
    /// to caller-supplied delegates, enabling ViewModel commands without coupling
    /// to specific UI types.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initialises a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The action to invoke when the command executes.</param>
        /// <param name="canExecute">
        /// An optional predicate that determines whether the command can execute.
        /// When <see langword="null"/> the command is always executable.
        /// </param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>Convenience constructor that accepts a parameterless action.</summary>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute is null ? null : _ => canExecute())
        {
        }

        /// <inheritdoc />
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <inheritdoc />
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> by invalidating the WPF command manager.
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
