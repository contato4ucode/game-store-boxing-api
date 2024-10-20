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

namespace GameStore.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductController : MainController
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;

    public ProductController(
        IProductService productService,
        IMapper mapper,
        IRedisCacheService redisCacheService,
        INotifier notifier,
        IAspNetUser user) : base(notifier, user)
    {
        _productService = productService;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
    }

    [HttpGet("{id:guid}")]
    [ClaimsAuthorize("Product", "Get")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var cacheKey = $"Product:{id}";
        var cachedProduct = await _redisCacheService.GetCacheValueAsync<ProductResponse>(cacheKey);
        if (cachedProduct != null)
        {
            return CustomResponse(cachedProduct);
        }

        return await HandleRequestAsync(
            async () =>
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return CustomResponse("Product not found", StatusCodes.Status404NotFound);
                }
                var productResponse = _mapper.Map<ProductResponse>(product);
                await _redisCacheService.SetCacheValueAsync(cacheKey, productResponse);
                return CustomResponse(productResponse);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllProducts()
    {
        return await HandleRequestAsync(
            async () =>
            {
                var cacheKey = "ProductList:All";
                var cachedProducts = await _redisCacheService.GetCacheValueAsync<IEnumerable<ProductResponse>>(cacheKey);
                if (cachedProducts != null)
                {
                    return CustomResponse(cachedProducts);
                }

                var products = await _productService.GetAllAsync();
                var productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);
                await _redisCacheService.SetCacheValueAsync(cacheKey, productResponses);
                return CustomResponse(productResponses);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPost]
    [ClaimsAuthorize("Product", "Add")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var product = _mapper.Map<Product>(request);
                var result = await _productService.CreateProductAsync(product);
                if (!result)
                {
                    return CustomResponse("Failed to create product", StatusCodes.Status400BadRequest);
                }
                var response = _mapper.Map<ProductResponse>(product);
                return CustomResponse(response, StatusCodes.Status201Created);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPut("{id:guid}")]
    [ClaimsAuthorize("Product", "Update")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductRequest request)
    {
        return await HandleRequestAsync(
            async () =>
            {
                var product = _mapper.Map<Product>(request);
                product.Id = id;
                await _productService.UpdateProductAsync(product);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }

    [HttpPatch("{id:guid}/status")]
    [ClaimsAuthorize("Product", "Delete")]
    public async Task<IActionResult> SoftDeleteProduct(Guid id)
    {
        return await HandleRequestAsync(
            async () =>
            {
                await _productService.SoftDeleteProductAsync(id);
                return CustomResponse(null, StatusCodes.Status204NoContent);
            },
            ex => CustomResponse(ex.Message, StatusCodes.Status400BadRequest)
        );
    }
}
