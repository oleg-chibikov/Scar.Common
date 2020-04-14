using System.Windows.Input;

namespace Scar.Common.MVVM.Commands
{
    public interface IRefreshableCommand : ICommand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "Method is more convenient")]
        void RaiseCanExecuteChanged();
    }
}
