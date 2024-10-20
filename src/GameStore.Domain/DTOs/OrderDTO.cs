namespace GameStore.Domain.DTOs;

public class OrderDTO
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public List<ProductDTO> Products { get; set; } = new();
}
