using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace C_C_Final.Presentation.Commands
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task>? _executeAsync;
        private readonly Action<object?>? _execute;
        private bool _isExecuting;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (_isExecuting)
            {
                return false;
            }

            return _canExecute?.Invoke(parameter) ?? true;
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                _isExecuting = true;
                if (_executeAsync != null)
                {
                    await _executeAsync(parameter).ConfigureAwait(false);
                }
                else
                {
                    _execute?.Invoke(parameter);
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
