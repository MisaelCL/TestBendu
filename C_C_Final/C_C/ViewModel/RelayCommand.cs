using System;
using System.Windows.Input;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Implementa un comando simple basado en delegados.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            _execute(parameter);
        }

        public void NotificarCambioPuedeEjecutar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
