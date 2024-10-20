using Asp.Versioning;
using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.Domain.Common;
using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using GameStore.Identity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : MainController
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;

    public OrderController(
        IOrderService orderService,
        IMapper mapper,
        IRedisCacheService redisCacheService,
        INotifier notifier,
        IAspNetUser user) : base(notifier, user)
    {
        _orderService = orderService;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
    }

    [HttpGet("{id:guid}")]
    [ClaimsAuthorize("Order", "Get")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var cacheKey = $"Order:{id}";
        var cachedOrder = await _redisCacheService.GetCacheValueAsync<OrderResponse>(cacheKey);
        if (cachedOrder != null)
        {
            return CustomResponse(cachedOrder);
        }

        return await HandleRequestAsync(
            async () =>
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return CustomResponse("Resource not found", StatusCodes.Status404NotFound);
                }
                var orderResponse = _mapper.Map<OrderResponse>(order);
                await _redisCacheService.SetCacheValueAsync(cacheKey, orderResponse);
                return CustomResponse(orderResponse);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public async Task<IActionResult> GetAllOrders([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        return await HandleRequestAsync(
            async () =>
            {
                string cacheKey = "OrderList:All";

                if (page.HasValue && pageSize.HasValue)
                {
                    var cachedOrders = await _redisCacheService.GetCacheValueAsync<PaginatedResponse<OrderResponse>>(cacheKey);
                    if (cachedOrders != null)
                    {
                        return CustomResponse(cachedOrders);
                    }

                    var orders = await _orderService.GetAllOrdersAsync();
                    var orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);
                    await _redisCacheService.SetCacheValueAsync(cacheKey, orderResponses);

                    return CustomResponse(orderResponses);
                }

                var cachedList = await _redisCacheService.GetCacheValueAsync<IEnumerable<OrderResponse>>(cacheKey);
                if (cachedList != null)
                {
                    return CustomResponse(cachedList);
                }

                var allOrders = await _orderService.GetAllOrdersAsync();
                var responses = _mapper.Map<IEnumerable<OrderResponse>>(allOrders);
                await _redisCacheService.SetCacheValueAsync(cacheKey, responses);

                return CustomResponse(responses);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPost]
    [ClaimsAuthorize("Order", "Add")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var order = await _orderService.CreateOrderAsync(request.CustomerId, request.ProductIds);
                if (order == null)
                {
                    return CustomResponse("Order creation failed", StatusCodes.Status400BadRequest);
                }

                var response = _mapper.Map<OrderResponse>(order);
                return CustomResponse(response, StatusCodes.Status201Created);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPut("{id:guid}")]
    [ClaimsAuthorize("Order", "Update")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderDTO orderDto)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var order = _mapper.Map<Order>(orderDto);
                order.Id = id;
                await _orderService.UpdateOrderAsync(order);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPatch("{id:guid}/status")]
    [ClaimsAuthorize("Order", "Delete")]
    public async Task<IActionResult> SoftDeleteOrder(Guid id)
    {
        return await HandleRequestAsync(
            async () =>
            {
                await _orderService.SoftDeleteOrderAsync(id);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }
}
