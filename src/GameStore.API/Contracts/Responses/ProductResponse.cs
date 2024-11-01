using GameStore.Domain.Models.ValueObjects;

namespace GameStore.API.Contracts.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Dimensions Dimensions { get; set; } = null!;
    public double Weight { get; set; }
    public decimal Price { get; set; }
}
