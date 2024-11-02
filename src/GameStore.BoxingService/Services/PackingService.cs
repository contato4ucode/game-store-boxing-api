using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;

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
        {
            throw new Exception("Order not found or contains no products.");
        }

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

        foreach (var orderId in orderIds)
        {
            var order = await _unitOfWork.Orders.GetById(orderId);
            if (order == null || !order.Products.Any())
            {
                responses.Add(new OrderPackingResponseDTO
                {
                    OrderId = orderId,
                    Boxes = new List<BoxAllocationDTO> {
                        new BoxAllocationDTO
                        {
                            BoxId = null,
                            Products = new List<string>(),
                            Observation = "Order not found or contains no products."
                        }
                    }
                });
                continue;
            }

            var availableBoxes = await _unitOfWork.Boxes.GetAll();
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

        var sortedProducts = products
            .OrderByDescending(p => p.Volume)
            .ToList();

        foreach (var product in sortedProducts)
        {
            var suitableBox = availableBoxes
                .Where(b =>
                    b.Dimensions.Height >= product.Dimensions.Height &&
                    b.Dimensions.Width >= product.Dimensions.Width &&
                    b.Dimensions.Length >= product.Dimensions.Length)
                .OrderBy(b => b.Volume)
                .FirstOrDefault();

            if (suitableBox == null)
            {
                allocations.Add(new BoxAllocationDTO
                {
                    BoxId = null,
                    Products = new List<string> { product.Name },
                    Observation = "Produto não cabe em nenhuma caixa disponível."
                });
                continue;
            }

            var allocation = allocations.FirstOrDefault(a => a.BoxId == suitableBox.Name);
            if (allocation == null)
            {
                allocation = new BoxAllocationDTO
                {
                    BoxId = suitableBox.Name,
                    Products = new List<string>()
                };
                allocations.Add(allocation);
            }

            allocation.Products.Add(product.Name);
        }

        return allocations;
    }
}
