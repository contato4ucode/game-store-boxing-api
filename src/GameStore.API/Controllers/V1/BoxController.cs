using Asp.Versioning;
using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Models;
using GameStore.Identity.Extensions;
using Microsoft.AspNetCore.Mvc;
using GameStore.API.Contracts.Requests;
using GameStore.Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace GameStore.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/boxes")]
public class BoxController : MainController
{
    private readonly IBoxService _boxService;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;

    public BoxController(
        IBoxService boxService,
        IMapper mapper,
        IRedisCacheService redisCacheService,
        INotifier notifier,
        IAspNetUser user) : base(notifier, user)
    {
        _boxService = boxService;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
    }

    [HttpGet("{id:guid}")]
    [ClaimsAuthorize("Box", "Get")]
    public async Task<IActionResult> GetBoxById(Guid id)
    {
        var cacheKey = $"Box:{id}";
        var cachedBox = await _redisCacheService.GetCacheValueAsync<BoxResponse>(cacheKey);
        if (cachedBox != null)
        {
            return CustomResponse(cachedBox);
        }

        return await HandleRequestAsync(
            async () =>
            {
                var box = await _boxService.GetByIdAsync(id);
                if (box == null)
                {
                    return CustomResponse("Box not found", StatusCodes.Status404NotFound);
                }
                var boxResponse = _mapper.Map<BoxResponse>(box);
                await _redisCacheService.SetCacheValueAsync(cacheKey, boxResponse);
                return CustomResponse(boxResponse);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public async Task<IActionResult> GetAllBoxes([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        return await HandleRequestAsync(
            async () =>
            {
                string cacheKey = $"BoxList:Page:{page ?? 1}:PageSize:{pageSize ?? 10}";

                var cachedBoxes = await _redisCacheService.GetCacheValueAsync<PaginatedResponse<BoxResponse>>(cacheKey);
                if (cachedBoxes != null)
                {
                    return CustomResponse(cachedBoxes);
                }

                var boxes = await _boxService.GetAllAsync();
                var boxResponses = _mapper.Map<IEnumerable<BoxResponse>>(boxes);

                var paginatedResponse = new PaginatedResponse<BoxResponse>(
                    boxResponses.Skip((page.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(10)).Take(pageSize.GetValueOrDefault(10)).ToList(),
                    boxResponses.Count(), page.GetValueOrDefault(1), pageSize.GetValueOrDefault(10)
                );

                await _redisCacheService.SetCacheValueAsync(cacheKey, paginatedResponse);

                return CustomResponse(paginatedResponse);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPost]
    [ClaimsAuthorize("Box", "Add")]
    public async Task<IActionResult> CreateBox([FromBody] BoxRequest request)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var box = _mapper.Map<Box>(request);
                var result = await _boxService.CreateBoxAsync(box, UserEmail);
                if (!result)
                {
                    return CustomResponse("Failed to create box", StatusCodes.Status400BadRequest);
                }
                var response = _mapper.Map<BoxResponse>(box);
                return CustomResponse(response, StatusCodes.Status201Created);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPut("{id:guid}")]
    [ClaimsAuthorize("Box", "Update")]
    public async Task<IActionResult> UpdateBox(Guid id, [FromBody] BoxRequest request)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var box = _mapper.Map<Box>(request);
                box.Id = id;
                await _boxService.CreateBoxAsync(box, UserEmail);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPatch("{id:guid}")]
    [ClaimsAuthorize("Box", "Delete")]
    public async Task<IActionResult> SoftDeleteBox(Guid id)
    {
        return await HandleRequestAsync(
            async () =>
            {
                await _boxService.SoftDeleteBoxAsync(id, UserEmail);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }
}
