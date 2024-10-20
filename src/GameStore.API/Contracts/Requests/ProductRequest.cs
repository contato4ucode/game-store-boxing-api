namespace GameStore.API.Contracts.Requests;

public class ProductRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public double Weight { get; set; }
    public decimal Price { get; set; }
}
