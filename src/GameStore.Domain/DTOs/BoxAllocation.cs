using GameStore.Domain.Models;

namespace GameStore.Domain.DTOs;

public class BoxAllocation
{
    public Box Box { get; set; }
    public List<Product> Products { get; set; }

    public BoxAllocation(Box box)
    {
        Box = box;
        Products = new List<Product>();
    }
}
