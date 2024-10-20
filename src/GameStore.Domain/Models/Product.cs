namespace GameStore.Domain.Models;

public class Product : EntityBase
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Length { get; private set; }
    public double Weight { get; private set; }
    public decimal Price { get; private set; }

    /// <summary>
    /// Calcula o volume do produto (altura x largura x comprimento).
    /// </summary>
    public int Volume => Height * Width * Length;

    public Product()
    {
    }

    public Product(string name, int height, int width, int length, double weight, decimal price, string? description = null)
    {
        Name = name;
        Height = height;
        Width = width;
        Length = length;
        Weight = weight;
        Price = price;
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Height}x{Width}x{Length} cm, {Weight} kg, R$ {Price}";
    }
}
