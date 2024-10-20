namespace GameStore.Domain.DTOs;

public class OrderPackingResponseDTO
{
    public int OrderId { get; set; }
    public List<BoxAllocationDTO> Boxes { get; set; } = new();
}

public class BoxAllocationDTO
{
    public string? BoxId { get; set; }
    public List<string> Products { get; set; } = new();
    public string? Observation { get; set; }
}
