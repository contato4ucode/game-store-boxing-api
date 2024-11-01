namespace GameStore.Domain.Models.ValueObjects;

public class Dimensions
{
    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Length { get; private set; }

    public int Volume => Height * Width * Length;

    public Dimensions(int height, int width, int length)
    {
        Height = height;
        Width = width;
        Length = length;
    }

    public override string ToString()
    {
        return $"{Height}x{Width}x{Length} cm";
    }
}
