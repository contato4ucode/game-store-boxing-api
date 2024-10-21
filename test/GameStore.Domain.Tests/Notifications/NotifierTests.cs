using FluentAssertions;
using FluentValidation.Results;
using GameStore.Domain.Notifications;

namespace GameStore.Domain.Tests.Notifications;

public class NotifierTests
{
    [Fact]
    public void HandleNotification_ShouldAddNotification()
    {
        var notifier = new Notifier();
        var notification = new Notification("Test message", NotificationType.Information);

        notifier.Handle(notification);

        notifier.HasNotification().Should().BeTrue();
        notifier.GetNotifications().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(notification);
    }

    [Fact]
    public void HandleMessage_ShouldAddInformationNotification()
    {
        var notifier = new Notifier();
        var message = "Test message";

        notifier.Handle(message);

        notifier.HasNotification().Should().BeTrue();
        notifier.GetNotifications().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new Notification(message, NotificationType.Information));
    }

    [Fact]
    public void HandleMessageWithType_ShouldAddTypedNotification()
    {
        var notifier = new Notifier();
        var message = "Test error";
        var type = NotificationType.Error;

        notifier.Handle(message, type);

        notifier.HasNotification().Should().BeTrue();
        notifier.GetNotifications().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new Notification(message, type));
    }

    [Fact]
    public void HandleException_ShouldAddErrorNotification()
    {
        var notifier = new Notifier();
        var exception = new InvalidOperationException("Invalid operation");

        notifier.Handle(exception);

        notifier.HasNotification().Should().BeTrue();
        notifier.GetNotifications().Should().ContainSingle()
            .Which.Message.Should().Contain("The operation could not be completed");
    }

    [Fact]
    public void Clean_ShouldRemoveAllNotifications()
    {
        var notifier = new Notifier();
        notifier.Handle("Test message");

        notifier.Clean();

        notifier.HasNotification().Should().BeFalse();
        notifier.GetNotifications().Should().BeEmpty();
    }

    [Fact]
    public void HandleValidationResult_ShouldAddNotifications()
    {
        var notifier = new Notifier();
        var validationResult = new ValidationResult(new[]
        {
                new ValidationFailure("Property1", "Error message 1"),
                new ValidationFailure("Property2", "Error message 2")
            });

        notifier.NotifyValidationErrors(validationResult);

        notifier.HasNotification().Should().BeTrue();
        notifier.GetNotifications().Should().HaveCount(2);
        notifier.GetNotifications().Select(n => n.Message).Should().Contain(new[] { "Error message 1", "Error message 2" });
    }
}
