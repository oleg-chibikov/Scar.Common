using System;

namespace Scar.Common.Notification
{
    public class NotificationSuppresser : IDisposable
    {
        readonly INotificationSupressable _notificationSuppressible;
        bool _disposedValue;

        public NotificationSuppresser(INotificationSupressable notificationSuppressible)
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
