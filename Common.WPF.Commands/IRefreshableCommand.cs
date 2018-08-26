using System.Windows.Input;

namespace Scar.Common.WPF.Commands
{
    public interface IRefreshableCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}