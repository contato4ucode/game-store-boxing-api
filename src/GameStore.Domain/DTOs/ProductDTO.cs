using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.DTOs;

public class ProductDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dimensions Dimensions { get; set; } = null!;
    public double Weight { get; set; }
    public decimal Price { get; set; }
    public int Volume => Dimensions.Volume;
}
