namespace GameStore.API.Contracts.Reponses;

public class BoxResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
}
