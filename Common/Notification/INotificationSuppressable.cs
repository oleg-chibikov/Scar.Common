namespace Scar.Common.Notification;

public interface INotificationSuppressable
{
    bool NotificationIsSuppressed { get; set; }

    NotificationSuppressor SuppressNotification();
}
