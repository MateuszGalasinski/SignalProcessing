using System;
using System.Windows.Input;

namespace UILogic.Base
{
    public class RelayCommand : IRaiseCanExecuteCommand
    {
        private readonly Action _targetExecuteMethod;
        private readonly Func<bool> _targetCanExecuteMethod;

        public event EventHandler CanExecuteChanged = delegate { };

        public RelayCommand(Action executeMethod)
        {
            _targetExecuteMethod = executeMethod;
        }

        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            _targetExecuteMethod = executeMethod;
            _targetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            if (_targetCanExecuteMethod != null)
            {
                return _targetCanExecuteMethod();
            }

            return _targetExecuteMethod != null;
        }

        void ICommand.Execute(object parameter)
        {
            _targetExecuteMethod?.Invoke();
        }

        #endregion ICommand Members
    }
}
