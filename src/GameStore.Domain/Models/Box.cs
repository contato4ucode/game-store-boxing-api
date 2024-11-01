using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.Models;

public class Box : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public Dimensions Dimensions { get; private set; } = new Dimensions(0, 0, 0);

    public int Volume => Dimensions.Volume;

    public Box() { }

    public Box(string name, Dimensions dimensions)
    {
        Name = name;
        Dimensions = dimensions;
    }

    public override string ToString()
    {
        return $"{Name} - {Dimensions}";
    }
}
