using System;

namespace Scar.Common.View.Contracts;

public interface IDisplayable
{
    event EventHandler Closed;

    event EventHandler Loaded;

    event EventHandler ContentRendered;

    bool? ShowDialog();

    void Show();

    void Close();

    void Restore();

    void AssociateDisposable(IDisposable disposable);

    bool UnassociateDisposable(IDisposable disposable);
}
