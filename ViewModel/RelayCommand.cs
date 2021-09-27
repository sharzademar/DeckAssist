using System;
using System.Windows.Input;

namespace DeckAssist.ViewModel
{
    internal class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter); //if _canExecute null, assumed to always be able to execute, otherwise run predicate
        }

        public void Execute(object parameter)
        {
            _execute(parameter ?? "<N/A>");
        }
    }
}