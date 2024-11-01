namespace GameStore.API.Contracts.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public List<ProductResponse> Products { get; set; } = [];
}
