using GameStore.Domain.Models;

namespace GameStore.Infrastructure.Tests.Helpers;

public static class EntityTestHelper
{
    public static Product CreateTestProduct(
        string name = "Test Product",
        int height = 10, int width = 10, int length = 10,
        double weight = 2.5, decimal price = 99.99m,
        string createdBy = "TestUser")
    {
        return new Product(name, height, width, length, weight, price)
        {
            CreatedByUser = createdBy
        };
    }

    public static Order CreateTestOrder(
        Guid customerId, List<Product> products,
        string createdBy = "TestUser")
    {
        return new Order(customerId, DateTime.UtcNow, products)
        {
            CreatedByUser = createdBy
        };
    }

    public static Box CreateTestBox(
        string name = "Test Box",
        int height = 50, int width = 50, int length = 50,
        string createdBy = "TestUser")
    {
        return new Box(name, height, width, length)
        {
            CreatedByUser = createdBy
        };
    }
}
