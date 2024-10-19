namespace GameStore.Domain.Models;

public class Product : EntityBase
{
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
}
