namespace GameStore.Domain.DTOs;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public List<BoxAllocation> Allocations { get; set; }

    public OrderResponse(Guid orderId, List<BoxAllocation> allocations)
    {
        OrderId = orderId;
        Allocations = allocations;
    }
}
