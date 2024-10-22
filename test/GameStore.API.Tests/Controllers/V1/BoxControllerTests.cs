using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.API.Controllers.V1;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace GameStore.API.Tests.Controllers.V1;

public class BoxControllerTests : BaseControllerTests<BoxController>
{
    private readonly IBoxService _boxServiceMock;
    private readonly IMapper _mapperMock;
    private readonly IRedisCacheService _redisCacheServiceMock;

    public BoxControllerTests() : base()
    {
        _boxServiceMock = Substitute.For<IBoxService>();
        _mapperMock = Substitute.For<IMapper>();
        _redisCacheServiceMock = Substitute.For<IRedisCacheService>();

        controller = new BoxController(
            _boxServiceMock,
            _mapperMock,
            _redisCacheServiceMock,
            _notifierMock,
            _userMock)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    private void VerifyErrorResponse(ObjectResult result, string expectedErrorMessage)
    {
        Assert.NotNull(result.Value);

        var responseObject = result.Value;
        var successProperty = responseObject.GetType().GetProperty("success");
        var errorsProperty = responseObject.GetType().GetProperty("errors");

        Assert.NotNull(successProperty);
        Assert.NotNull(errorsProperty);

        Assert.False((bool)successProperty.GetValue(responseObject));
        Assert.Equal(expectedErrorMessage, errorsProperty.GetValue(responseObject) as string);
    }

    [Fact]
    public async Task GetBoxById_ShouldReturnBox_WhenBoxExists()
    {
        var boxId = Guid.NewGuid();
        var box = new Box("Box 1", 10, 10, 10);
        var boxResponse = new BoxResponse { Name = "Box 1", Height = 10, Width = 10, Length = 10 };
        var cacheKey = $"Box:{boxId}";

        _redisCacheServiceMock.GetCacheValueAsync<BoxResponse>(cacheKey).Returns((BoxResponse)null);
        _boxServiceMock.GetByIdAsync(boxId).Returns(Task.FromResult(box));
        _mapperMock.Map<BoxResponse>(box).Returns(boxResponse);

        var result = await controller.GetBoxById(boxId);
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(boxResponse, response.GetType().GetProperty("data").GetValue(response));

        await _redisCacheServiceMock.Received(1).GetCacheValueAsync<BoxResponse>(cacheKey);
        await _redisCacheServiceMock.Received(1).SetCacheValueAsync(cacheKey, boxResponse);
    }

    [Fact]
    public async Task GetBoxById_ShouldReturnNotFound_WhenBoxDoesNotExist()
    {
        var boxId = Guid.NewGuid();
        var cacheKey = $"Box:{boxId}";

        _redisCacheServiceMock.GetCacheValueAsync<BoxResponse>(cacheKey).Returns((BoxResponse)null);
        _boxServiceMock.GetByIdAsync(boxId).Returns(Task.FromResult<Box>(null));

        var result = await controller.GetBoxById(boxId);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        VerifyErrorResponse(notFoundResult, "Box not found");

        await _redisCacheServiceMock.Received(1).GetCacheValueAsync<BoxResponse>(cacheKey);
    }

    [Fact]
    public async Task GetAllBoxes_ShouldReturnBoxes_WhenBoxesExist()
    {
        var boxes = new List<Box> { new Box("Box 1", 10, 10, 10) };
        var boxResponses = new List<BoxResponse> { new BoxResponse { Name = "Box 1", Height = 10, Width = 10, Length = 10 } };
        var cacheKey = "BoxList:All";

        _redisCacheServiceMock.GetCacheValueAsync<IEnumerable<BoxResponse>>(cacheKey).Returns((IEnumerable<BoxResponse>)null);
        _boxServiceMock.GetAllAsync().Returns(Task.FromResult((IEnumerable<Box>)boxes));
        _mapperMock.Map<IEnumerable<BoxResponse>>(boxes).Returns(boxResponses);

        var result = await controller.GetAllBoxes(1, 20);
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(boxResponses, response.GetType().GetProperty("data").GetValue(response) as IEnumerable<BoxResponse>);

        await _redisCacheServiceMock.Received(1).GetCacheValueAsync<IEnumerable<BoxResponse>>(cacheKey);
        await _redisCacheServiceMock.Received(1).SetCacheValueAsync(cacheKey, boxResponses);
    }

    [Fact]
    public async Task CreateBox_ShouldReturnCreated_WhenBoxIsValid()
    {
        // Arrange
        var boxRequest = new BoxRequest { Name = "Box 1", Height = 10, Width = 10, Length = 10 };
        var box = new Box("Box 1", 10, 10, 10);
        var boxResponse = new BoxResponse { Name = "Box 1", Height = 10, Width = 10, Length = 10 };
        var userEmail = "test@email.com";

        _mapperMock.Map<Box>(boxRequest).Returns(box);
        _boxServiceMock.CreateBoxAsync(box, userEmail).Returns(true);
        _mapperMock.Map<BoxResponse>(box).Returns(boxResponse);

        // Act
        var result = await controller.CreateBox(boxRequest);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.NotNull(createdResult.Value);

        var response = createdResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(boxResponse, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public async Task CreateBox_ShouldReturnBadRequest_WhenCreationFails()
    {
        var boxRequest = new BoxRequest { Name = "Box 1", Height = 10, Width = 10, Length = 10 };
        var box = new Box("Box 1", 10, 10, 10);
        var userEmail = "test@email";

        _mapperMock.Map<Box>(boxRequest).Returns(box);
        _boxServiceMock.CreateBoxAsync(box, userEmail).Returns(Task.FromResult(false));

        var result = await controller.CreateBox(boxRequest);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        VerifyErrorResponse(badRequestResult, "Failed to create box");
    }

    [Fact]
    public async Task UpdateBox_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        var boxRequest = new BoxRequest { Name = "Box 1", Height = 10, Width = 10, Length = 10 };
        var boxId = Guid.NewGuid();
        var box = new Box("Box 1", 10, 10, 10) { Id = boxId };
        var userEmail = "test@email";

        _mapperMock.Map<Box>(boxRequest).Returns(box);
        _boxServiceMock.CreateBoxAsync(box, userEmail).Returns(Task.FromResult(true));

        var result = await controller.UpdateBox(boxId, boxRequest);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SoftDeleteBox_ShouldReturnNoContent_WhenSoftDeleteIsSuccessful()
    {
        // Arrange
        var boxId = Guid.NewGuid();
        var userEmail = "test@email";

        _boxServiceMock.SoftDeleteBoxAsync(boxId, userEmail).Returns(Task.FromResult(true));

        // Act
        var result = await controller.SoftDeleteBox(boxId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _boxServiceMock.Received(1).SoftDeleteBoxAsync(boxId, userEmail);
    }
}
