using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;

namespace GameStore.BoxingService.Services;

public class PackingService : IPackingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PackingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderPackingResponseDTO> ProcessOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetById(orderId);
        if (order == null || !order.Products.Any())
            return CreateOrderResponseWithObservation(orderId, "Order not found or contains no products.");

        var availableBoxes = await _unitOfWork.Boxes.GetAll();
        var allocations = AllocateProductsToBoxes(order.Products, availableBoxes);

        return new OrderPackingResponseDTO
        {
            OrderId = orderId,
            Boxes = allocations
        };
    }

    public async Task<List<OrderPackingResponseDTO>> ProcessOrdersAsync(List<Guid> orderIds)
    {
        var responses = new List<OrderPackingResponseDTO>();
        var availableBoxes = await _unitOfWork.Boxes.GetAll();

        foreach (var orderId in orderIds)
        {
            var order = await _unitOfWork.Orders.GetById(orderId);
            if (order == null || !order.Products.Any())
            {
                responses.Add(CreateOrderResponseWithObservation(orderId, "Order not found or contains no products."));
                continue;
            }

            var allocations = AllocateProductsToBoxes(order.Products, availableBoxes);
            responses.Add(new OrderPackingResponseDTO
            {
                OrderId = orderId,
                Boxes = allocations
            });
        }

        return responses;
    }

    private List<BoxAllocationDTO> AllocateProductsToBoxes(List<Product> products, IEnumerable<Box> availableBoxes)
    {
        var allocations = new List<BoxAllocationDTO>();
        var remainingProducts = new List<Product>(products);

        foreach (var box in availableBoxes.OrderBy(b => b.Volume))
        {
            var boxAllocation = new BoxAllocationDTO
            {
                BoxId = box.Name,
                Products = new List<string>()
            };

            foreach (var product in remainingProducts.ToList())
            {
                if (FitsInBox(product.Dimensions, box.Dimensions))
                {
                    boxAllocation.Products.Add(product.Name);
                    remainingProducts.Remove(product);
                }
            }

            if (boxAllocation.Products.Any())
            {
                allocations.Add(boxAllocation);
            }

            if (!remainingProducts.Any()) break;
        }

        if (remainingProducts.Any())
        {
            allocations.AddRange(remainingProducts.Select(product => new BoxAllocationDTO
            {
                BoxId = null,
                Products = new List<string> { product.Name },
                Observation = "Produto não cabe em nenhuma caixa disponível."
            }));
        }

        return allocations;
    }

    private bool FitsInBox(Dimensions productDims, Dimensions boxDims)
    {
        return productDims.Height <= boxDims.Height &&
               productDims.Width <= boxDims.Width &&
               productDims.Length <= boxDims.Length;
    }

    private OrderPackingResponseDTO CreateOrderResponseWithObservation(Guid orderId, string observation)
    {
        return new OrderPackingResponseDTO
        {
            OrderId = orderId,
            Boxes = new List<BoxAllocationDTO>
            {
                new BoxAllocationDTO
                {
                    BoxId = null,
                    Products = new List<string>(),
                    Observation = observation
                }
            }
        };
    }
}
