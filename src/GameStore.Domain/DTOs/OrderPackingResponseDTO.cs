namespace GameStore.Domain.DTOs;

public class OrderPackingResponseDTO
{
    public Guid OrderId { get; set; }
    public List<BoxAllocationDTO> Boxes { get; set; }
}

public class BoxAllocationDTO
{
    public string? BoxId { get; set; }
    public List<string> Products { get; set; }
    public string? Observation { get; set; }
}
