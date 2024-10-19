namespace GameStore.Domain.Models;

public class Box : EntityBase
{
    public string Name { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
}
