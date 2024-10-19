using FluentValidation;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Models;
using GameStore.Domain.Notifications;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GameStore.SharedServices.Services;

public class BaseService
{
    protected readonly INotifier _notifier;

    public BaseService(INotifier notifier)
    {
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    protected async Task<List<string>> ExecuteValidationAsync<TV, TE>(TV validation, TE entity)
        where TV : AbstractValidator<TE>
        where TE : EntityBase
    {
        var validationResult = await validation.ValidateAsync(entity);
        var errors = new List<string>();

        if (validationResult.IsValid) return errors;

        foreach (var error in validationResult.Errors)
        {
            var errorMessage = $"Property: {error.PropertyName} - Error: {error.ErrorMessage}";
            _notifier.Handle(errorMessage);
            errors.Add(errorMessage);
        }

        return errors;
    }

    protected void HandleException(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        string friendlyMessage = GenerateFriendlyMessage(exception);
        _notifier.Handle(friendlyMessage, NotificationType.Error);
    }

    private string GenerateFriendlyMessage(Exception exception) => exception switch
    {
        ArgumentNullException argNullException =>
            $"A required argument was not provided: {argNullException.ParamName}. Please check and try again.",
        ArgumentException argException =>
            $"There was an issue with the provided argument: {argException.Message}. Please correct it and try again.",
        InvalidOperationException invalidOpException =>
            $"The operation could not be completed: {invalidOpException.Message}. Please ensure all preconditions are met and try again.",
        DbUpdateException dbUpdateException =>
            dbUpdateException.GetBaseException() is SqlException sqlException ?
                TranslateSqlException(sqlException) :
                $"Database update error: {dbUpdateException.InnerException?.Message ?? dbUpdateException.Message}. Please contact support if the issue persists.",
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
}
