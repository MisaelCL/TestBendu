using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProyectoEjemplo.ViewModel
{
    public class ViewModelCommand : ICommand
    {
        //Representa el método que se ejecuta cuando se llama
        //Execute
        private readonly Action<object> _executeAction;

        //Representa una función que devuelve bool y nos dice
        // si el comando se puede ejecutar.
        private readonly Predicate<object> _canExecuteAction;

        //Constructores 
        //Este constructor permite ejecutar una acción
        //sin revisión previa
        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = null;
        }

        //Se revisa si se puede ejecutar la acción,
        //En caso de que la condición sea veradera,
        //la acción se ejecuta
        public ViewModelCommand(
            Action<object> executeAction,
            Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested  += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //Indica si se puede ejecutar el comando
        //Si _canExecute es null --> devuelve true (el comando siempre se puede ejecutar)
        //Si no es null --> se llama al predicado para que decida.
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null ? true: _canExecuteAction(parameter);
        }

        //Lo que se va a ejecutar
        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
