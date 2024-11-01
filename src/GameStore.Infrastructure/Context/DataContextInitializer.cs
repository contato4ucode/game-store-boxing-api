using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Infrastructure.Context;

public static class DataContextInitializer
{
    public static void Initialize(DataContext context)
    {
        if (!context.Boxes.Any())
        {
            var boxes = new List<Box>
            {
                new Box("Box 1", new Dimensions(30, 40, 80))
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Box("Box 2", new Dimensions(80, 50, 40))
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Box("Box 3", new Dimensions(50, 80, 60))
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                }
            };

            context.Boxes.AddRange(boxes);
        }

        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product("PS5", new Dimensions(40, 10, 25), 4.5, 4999.99m, "Console de videogame")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Volante", new Dimensions(40, 30, 30), 3.2, 599.99m, "Volante de corrida para jogos")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Joystick", new Dimensions(15, 20, 10), 0.6, 199.99m, "Controle de videogame")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Fifa 24", new Dimensions(10, 30, 10), 0.2, 299.99m, "Jogo de futebol")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Call of Duty", new Dimensions(30, 15, 10), 0.3, 399.99m, "Jogo de tiro")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Headset", new Dimensions(25, 15, 20), 0.7, 299.99m, "Fone de ouvido para jogos")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Mouse Gamer", new Dimensions(5, 8, 12), 0.15, 149.99m, "Mouse de alta precisão")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Teclado Mecânico", new Dimensions(4, 45, 15), 1.2, 449.99m, "Teclado para jogos")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Cadeira Gamer", new Dimensions(120, 60, 70), 15.0, 1299.99m, "Cadeira ergonômica para jogos")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Webcam", new Dimensions(7, 10, 5), 0.1, 199.99m, "Câmera de alta definição")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Microfone", new Dimensions(25, 10, 10), 0.3, 299.99m, "Microfone condensador")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Monitor", new Dimensions(50, 60, 20), 7.5, 2499.99m, "Monitor 4K")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Notebook", new Dimensions(2, 35, 25), 1.8, 4999.99m, "Notebook de alta performance")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Jogo de Cabos", new Dimensions(5, 15, 10), 0.05, 49.99m, "Kit de cabos diversos")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Controle Xbox", new Dimensions(10, 15, 10), 0.5, 199.99m, "Controle para console Xbox")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Carregador", new Dimensions(3, 8, 8), 0.2, 99.99m, "Carregador universal")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Tablet", new Dimensions(1, 25, 17), 0.4, 999.99m, "Tablet de 10 polegadas")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("HD Externo", new Dimensions(2, 8, 12), 0.3, 349.99m, "HD de armazenamento externo")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                },
                new Product("Pendrive", new Dimensions(1, 2, 5), 0.02, 49.99m, "Pendrive de 64GB")
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUser = "System"
                }
            };

            context.Products.AddRange(products);
        }

        context.SaveChanges();
    }
}
