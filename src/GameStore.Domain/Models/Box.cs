namespace GameStore.Domain.Models;

public class Box : EntityBase
{
    public string Name { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Length { get; private set; }

    /// <summary>
    /// Calcula o volume da caixa (altura x largura x comprimento).
    /// </summary>
    public int Volume => Height * Width * Length;

    public Box(string name, int height, int width, int length)
    {
        Name = name;
        Height = height;
        Width = width;
        Length = length;
    }

    public override string ToString()
    {
        return $"{Name} - {Height}x{Width}x{Length} cm";
    }
}
