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
                    b.Height >= product.Height &&
                    b.Width >= product.Width &&
                    b.Length >= product.Length)
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
