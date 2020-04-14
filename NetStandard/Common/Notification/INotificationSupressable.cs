namespace Scar.Common.Notification
{
    public interface INotificationSupressable
    {
        bool NotificationIsSupressed { get; set; }

        NotificationSuppresser SupressNotification();
    }
}
