using System;

namespace Scar.Common.Notification
{
    public class NotificationSuppressor : IDisposable
    {
        readonly INotificationSupressable _notificationSuppressible;
        bool _disposedValue;

        public NotificationSuppressor(INotificationSupressable notificationSuppressible)
        {
            _notificationSuppressible = notificationSuppressible ?? throw new ArgumentNullException(nameof(notificationSuppressible));
            notificationSuppressible.NotificationIsSupressed = true;
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
                    _notificationSuppressible.NotificationIsSupressed = false;
                }

                _disposedValue = true;
            }
        }
    }
}
