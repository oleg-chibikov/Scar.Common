using System;

namespace Scar.Common.Notification
{
    public class NotificationSupresser : IDisposable
    {
        private readonly INotificationSupressable _notificationSupressable;

        public NotificationSupresser(INotificationSupressable notificationSupressable)
        {
            _notificationSupressable = notificationSupressable ?? throw new ArgumentNullException(nameof(notificationSupressable));
            notificationSupressable.NotificationIsSupressed = true;
        }

        public virtual void Dispose()
        {
            _notificationSupressable.NotificationIsSupressed = false;
        }
    }
}