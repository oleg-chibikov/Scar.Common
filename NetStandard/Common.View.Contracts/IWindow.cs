using System;

namespace Scar.Common.View.Contracts
{
    public interface IDisplayable
    {
        bool? ShowDialog();

        void Show();

        void Close();

        void Restore();

        void AssociateDisposable(IDisposable disposable);

        bool UnassociateDisposable(IDisposable disposable);

        event EventHandler SizeChanged;

        event EventHandler Closed;

        event EventHandler Loaded;
    }
}