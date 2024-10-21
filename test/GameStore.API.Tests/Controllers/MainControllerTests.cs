using GameStore.API.Controllers;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using System.Reflection;

namespace GameStore.API.Tests.Controllers;

public class MainControllerTests
{
    private readonly MainController _mainController;
    private readonly INotifier _notifierMock;
    private readonly IAspNetUser _userMock;
    private readonly DefaultHttpContext _httpContext;

    public MainControllerTests()
    {
        _notifierMock = Substitute.For<INotifier>();
        _userMock = Substitute.For<IAspNetUser>();
        _httpContext = new DefaultHttpContext();
        _mainController = Substitute.ForPartsOf<MainController>(_notifierMock, _userMock);
        _mainController.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }

    private ActionResult InvokeCustomResponse(object result = null, int? statusCode = null)
    {
        var methodInfo = typeof(MainController).GetMethod("CustomResponse", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(object), typeof(int?) }, null);
        return (ActionResult)methodInfo.Invoke(_mainController, new object[] { result, statusCode });
    }

    private ActionResult InvokeCustomResponse(ModelStateDictionary modelState)
    {
        var methodInfo = typeof(MainController).GetMethod("CustomResponse", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(ModelStateDictionary) }, null);
        return (ActionResult)methodInfo.Invoke(_mainController, new object[] { modelState });
    }

    [Fact]
    public void CustomResponse_ShouldReturnOk_WhenValidOperation()
    {
        // Arrange
        var resultData = new { Name = "Test" };

        // Act
        var result = InvokeCustomResponse(resultData);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(resultData, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public void CustomResponse_ShouldReturnBadRequest_WhenInvalidOperation()
    {
        // Arrange
        _notifierMock.GetNotifications().Returns(new List<Notification>
        {
            new Notification("Error", NotificationType.Error)
        });

        // Act
        var result = InvokeCustomResponse();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        var errors = (List<string>)response.GetType().GetProperty("errors").GetValue(response);
        Assert.Contains("Error", errors);
    }

    [Fact]
    public void CustomResponse_WithModelState_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = InvokeCustomResponse(modelState);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        var errors = (List<string>)response.GetType().GetProperty("errors").GetValue(response);
        Assert.Contains("The Name field is required.", errors);
    }

    [Fact]
    public void CustomResponse_ShouldReturnNotFound_WhenResourceIsNullAndMethodIsGet()
    {
        // Arrange
        _httpContext.Request.Method = "GET";

        // Act
        var result = InvokeCustomResponse(null);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
        var response = notFoundResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        var errors = (string)response.GetType().GetProperty("errors").GetValue(response);
        Assert.Equal("Resource not found", errors);
    }
}
