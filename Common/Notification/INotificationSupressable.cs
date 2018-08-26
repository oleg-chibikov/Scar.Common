using JetBrains.Annotations;

namespace Scar.Common.Notification
{
    public interface INotificationSupressable
    {
        bool NotificationIsSupressed { get; set; }

        [NotNull]
        NotificationSupresser SupressNotification();
    }
}