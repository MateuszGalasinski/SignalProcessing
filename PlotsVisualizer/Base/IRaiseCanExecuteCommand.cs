using System.Windows.Input;

namespace UILogic.Base
{
    public interface IRaiseCanExecuteCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
