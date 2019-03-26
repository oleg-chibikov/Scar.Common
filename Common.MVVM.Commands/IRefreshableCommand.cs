using System.Windows.Input;

namespace Scar.Common.MVVM.Commands
{
    public interface IRefreshableCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}