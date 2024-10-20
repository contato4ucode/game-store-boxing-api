namespace GameStore.Domain.DTOs;

public class OrderPackingRequestDTO
{
    public int OrderId { get; set; }
    public List<ProductRequestDTO> Products { get; set; } = new();
}

public class ProductRequestDTO
{
    public string ProductId { get; set; }
    public DimensionsDTO Dimensions { get; set; }
}

public class DimensionsDTO
{
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
}
