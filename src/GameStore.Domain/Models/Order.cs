namespace GameStore.Domain.Models;

public class Order : EntityBase
{
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public List<Product> Products { get; private set; } = new();

    public Order()
    {
    }

    public Order(Guid customerId, DateTime orderDate, List<Product> products)
    {
        CustomerId = customerId;
        OrderDate = orderDate;
        Products = products ?? throw new ArgumentNullException(nameof(products));

        if (!products.Any())
            throw new ArgumentException("An order must contain at least one product.");
    }

    public decimal TotalPrice => Products.Sum(p => p.Price);
}
