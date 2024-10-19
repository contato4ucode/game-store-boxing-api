namespace GameStore.Domain.Notifications;

public class Notification
{
    public Notification(string message, NotificationType type)
    {
        Message = message;
        Type = type;
    }

    public string Message { get; }
    public NotificationType Type { get; }
}

public enum NotificationType
{
    Information,
    Error
}
