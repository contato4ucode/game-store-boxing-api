using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GameStore.API.Controllers;

[ApiController]
public abstract class MainController : Controller
{
    protected readonly INotifier _notifier;
    protected readonly IAspNetUser _user;

    protected Guid UserId { get; set; }
    protected string UserEmail { get; set; }

    protected MainController(INotifier notifier, IAspNetUser user)
    {
        _notifier = notifier;

        if (user.IsAuthenticated())
        {
            UserId = user.GetUserId();
            UserEmail = user.GetUserEmail();
        }
    }

    protected ActionResult CustomResponse(object? result = null, int? statusCode = null)
    {
        if (!ValidOperation())
            return HandleErrorResponse();

        if (statusCode.HasValue)
        {
            return statusCode.Value switch
            {
                StatusCodes.Status404NotFound => NotFound(new { success = false, errors = result ?? "Resource not found" }),
                StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, new { success = true, data = result }),
                StatusCodes.Status204NoContent => NoContent(),
                StatusCodes.Status400BadRequest => BadRequest(new { success = false, errors = result ?? "Bad request" }),
                _ => StatusCode(statusCode.Value, new { success = true, data = result })
            };
        }

        return HttpContext.Request.Method switch
        {
            "GET" => result == null ? NotFound(new { success = false, errors = "Resource not found" }) : Ok(new { success = true, data = result }),
            "POST" => result == null ? Conflict(new { success = false, errors = "Resource conflict" }) : StatusCode(201, new { success = true, data = result }),
            "PUT" or "PATCH" => result == null ? NotFound(new { success = false, errors = "Resource not found" }) : Ok(new { success = true, data = result }),
            "DELETE" => NoContent(),
            _ => Ok(new { success = true, data = result })
        };
    }

    protected ActionResult CustomResponse(string errorMessage)
    {
        return BadRequest(new { success = false, errors = errorMessage });
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        if (modelState != null && !modelState.IsValid)
        {
            NotifyErrorInvalidModel(modelState);
            return BadRequest(new
            {
                success = false,
                errors = modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        return CustomResponse();
    }

    protected bool ValidOperation() => !_notifier.GetNotifications().Any(n => n.Type == NotificationType.Error);

    private void NotifyErrorInvalidModel(ModelStateDictionary modelState)
    {
        if (modelState == null) return;

        var errors = modelState.Values.SelectMany(e => e.Errors);
        foreach (var error in errors)
        {
            var errorMsg = error.Exception?.Message ?? error.ErrorMessage;
            var errorType = NotificationType.Error;
            NotifyError(errorMsg, errorType);
        }
    }

    protected void NotifyError(string message, NotificationType type = NotificationType.Error) => _notifier.Handle(new Notification(message, type));

    protected void HandleException(Exception exception) => _notifier.HandleException(exception);

    private ActionResult HandleErrorResponse()
    {
        var errors = _notifier.GetNotifications()
            .Where(n => n.Type == NotificationType.Error)
            .Select(n => n.Message)
            .ToList();

        if (IsNotFoundError(errors))
        {
            return NotFound(new { success = false, errors });
        }

        if (IsBadRequestError(errors))
        {
            return BadRequest(new { success = false, errors });
        }

        return StatusCode(500, new { success = false, errors, message = "An unexpected error occurred." });
    }

    private static bool IsNotFoundError(IEnumerable<string> errors) =>
        errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase));

    private static bool IsBadRequestError(IEnumerable<string> errors) =>
        errors.Any(e =>
            e.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
            e.Contains("cannot", StringComparison.OrdinalIgnoreCase) ||
            e.Contains("Error", StringComparison.OrdinalIgnoreCase));

    protected async Task<IActionResult> HandleRequestAsync(Func<Task<IActionResult>> operation, Func<Exception, IActionResult> handleException)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            return handleException(ex);
        }
    }
}
