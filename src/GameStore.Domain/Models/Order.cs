namespace GameStore.Domain.Models;

public class Order : EntityBase
{
    public List<Product> Products { get; set; } = new();
}
