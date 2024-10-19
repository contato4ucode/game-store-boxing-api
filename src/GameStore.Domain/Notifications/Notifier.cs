using FluentValidation.Results;
using GameStore.Domain.Interfaces.Notifications;
using Microsoft.Data.SqlClient;

namespace GameStore.Domain.Notifications;

public class Notifier : INotifier
{
    private readonly List<Notification> _notifications;

    public Notifier()
    {
        _notifications = new List<Notification>();
    }

    public IReadOnlyList<Notification> GetNotifications() => _notifications.AsReadOnly();

    public bool HasNotification() => _notifications.Any();

    public void Handle(Notification notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        _notifications.Add(notification);
    }

    public void Handle(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        _notifications.Add(new Notification(message, NotificationType.Information));
    }

    public void Handle(string message, NotificationType type)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        _notifications.Add(new Notification(message, type));
    }

    public void Handle(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        HandleException(exception);
    }

    public void HandleException(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        string friendlyMessage = GenerateFriendlyMessage(exception);
        _notifications.Add(new Notification(friendlyMessage, NotificationType.Error));
    }

    private string GenerateFriendlyMessage(Exception exception) => exception switch
    {
        ArgumentNullException argNullException => $"A required argument was not provided: {argNullException.ParamName}. Please check and try again.",
        ArgumentException argException => $"There was an issue with the provided argument: {argException.Message}. Please correct it and try again.",
        InvalidOperationException invalidOpException => $"The operation could not be completed: {invalidOpException.Message}. Please ensure all preconditions are met and try again.",
        SqlException sqlException => TranslateSqlException(sqlException),
        _ => $"An unexpected error occurred: {exception.Message}. Please try again later or contact support."
    };

    private string TranslateSqlException(SqlException sqlException) => sqlException.Number switch
    {
        8152 => "The data provided is too long for the database column. Please shorten the input and try again.",
        2627 => "A unique constraint was violated. This usually means the data you are trying to enter already exists.",
        547 => "A foreign key constraint was violated. Please ensure all related data exists.",
        2601 => "A duplicate key error occurred. Please ensure the data is unique and try again.",
        _ => $"A database error occurred: {sqlException.Message}. Please contact support if the issue persists."
    };

    public void NotifyValidationErrors(ValidationResult validationResult)
    {
        if (validationResult == null) throw new ArgumentNullException(nameof(validationResult));
        foreach (var error in validationResult.Errors)
        {
            Handle(error.ErrorMessage, NotificationType.Error);
        }
    }

    public void Clean() => _notifications.Clear();
}
