using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.DTOs;

public class BoxDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dimensions Dimensions { get; set; } = null!;
    public int Volume => Dimensions.Volume;
}
