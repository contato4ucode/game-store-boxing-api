using GameStore.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Mappings;

public class OrderMapping : EntityBaseMapping<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.Property(o => o.CustomerId)
               .IsRequired();

        builder.Property(o => o.OrderDate)
               .IsRequired();

        builder.Ignore(o => o.TotalPrice);

        builder.HasMany(o => o.Products)
               .WithOne()
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Orders");
    }
}
