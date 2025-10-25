using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace C_C.ViewModel.Commands;

public sealed class RelayCommand : ICommand, IDisposable
{
    private readonly Func<object?, CancellationToken, Task>? _executeAsync;
    private readonly Action<object?>? _executeSync;
    private readonly Func<object?, bool>? _canExecute;
    private CancellationTokenSource? _cts;

    public RelayCommand(Func<object?, CancellationToken, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _executeSync = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        if (_executeAsync is not null)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            try
            {
                await _executeAsync(parameter, _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignorar cancelaciones explícitas
            }
        }
        else
        {
            _executeSync!(parameter);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void Cancel()
    {
        if (_cts is null)
        {
            return;
        }

        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
    }
}
