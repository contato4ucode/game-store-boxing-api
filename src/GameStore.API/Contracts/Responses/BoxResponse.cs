using GameStore.Domain.Models.ValueObjects;

namespace GameStore.API.Contracts.Responses;

public class BoxResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Dimensions Dimensions { get; set; } = null!;
}
