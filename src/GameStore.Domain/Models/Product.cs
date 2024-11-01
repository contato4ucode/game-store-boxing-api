using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.Models;

public class Product : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Dimensions Dimensions { get; private set; } = new Dimensions(0, 0, 0);
    public double Weight { get; private set; }
    public decimal Price { get; private set; }

    public int Volume => Dimensions.Volume;

    public Product() { }

    public Product(string name, Dimensions dimensions, double weight, decimal price, string? description = null)
    {
        Name = name;
        Dimensions = dimensions;
        Weight = weight;
        Price = price;
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Dimensions}, {Weight} kg, R$ {Price}";
    }
}
