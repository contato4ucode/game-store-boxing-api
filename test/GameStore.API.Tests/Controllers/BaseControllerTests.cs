using GameStore.API.Controllers;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using NSubstitute;

namespace GameStore.API.Tests.Controllers;

public abstract class BaseControllerTests<T> where T : MainController
{
    protected T controller;
    protected INotifier _notifierMock;
    protected IAspNetUser _userMock;
    protected DefaultHttpContext _httpContext;

    public BaseControllerTests()
    {
        _notifierMock = Substitute.For<INotifier>();
        _userMock = Substitute.For<IAspNetUser>();
        _httpContext = new DefaultHttpContext();
    }

    protected ActionResult InvokeCustomResponse(object result = null, int? statusCode = null)
    {
        var methodInfo = typeof(MainController).GetMethod("CustomResponse", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(object), typeof(int?) }, null);
        return (ActionResult)methodInfo.Invoke(controller, new object[] { result, statusCode });
    }

    protected ActionResult InvokeCustomResponse(ModelStateDictionary modelState)
    {
        var methodInfo = typeof(MainController).GetMethod("CustomResponse", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(ModelStateDictionary) }, null);
        return (ActionResult)methodInfo.Invoke(controller, new object[] { modelState });
    }
}
