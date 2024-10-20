namespace GameStore.API.Contracts.Requests;

public class BoxRequest
{
    public string Name { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
}
