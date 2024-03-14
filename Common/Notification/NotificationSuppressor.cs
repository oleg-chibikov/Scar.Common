using System;

namespace Scar.Common.Notification;

public class NotificationSuppressor : IDisposable
{
    readonly INotificationSuppressable _notificationSuppressible;
    bool _disposedValue;

    public NotificationSuppressor(INotificationSuppressable notificationSuppressible)
    {
        _notificationSuppressible = notificationSuppressible ?? throw new ArgumentNullException(nameof(notificationSuppressible));
        notificationSuppressible.NotificationIsSuppressed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _notificationSuppressible.NotificationIsSuppressed = false;
            }

            _disposedValue = true;
        }
    }
}