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

    public async Task<List<OrderPackingResponseDTO>> ProcessOrdersAsync(List<OrderPackingRequestDTO> orders)
    {
        var availableBoxes = await _unitOfWork.Boxes.GetAll();
        var responses = new List<OrderPackingResponseDTO>();

        foreach (var order in orders)
        {
            var allocations = AllocateProductsToBoxes(order.Products, availableBoxes);
            responses.Add(new OrderPackingResponseDTO
            {
                OrderId = order.OrderId,
                Boxes = allocations
            });
        }

        return responses;
    }

    private List<BoxAllocationDTO> AllocateProductsToBoxes(List<ProductRequestDTO> products, IEnumerable<Box> availableBoxes)
    {
        var allocations = new List<BoxAllocationDTO>();
        var sortedProducts = products
            .OrderByDescending(p => p.Dimensions.Height * p.Dimensions.Width * p.Dimensions.Length)
            .ToList();

        foreach (var product in sortedProducts)
        {
            var suitableBox = availableBoxes
                .Where(b =>
                    b.Height >= product.Dimensions.Height &&
                    b.Width >= product.Dimensions.Width &&
                    b.Length >= product.Dimensions.Length)
                .OrderBy(b => b.Volume)
                .FirstOrDefault();

            if (suitableBox == null)
            {
                allocations.Add(new BoxAllocationDTO
                {
                    BoxId = null,
                    Products = new List<string> { product.ProductId },
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

            allocation.Products.Add(product.ProductId);
        }

        return allocations;
    }
}
