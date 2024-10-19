using GameStore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Mappings;

public class ProductMapping : EntityBaseMapping<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Height).IsRequired();
        builder.Property(p => p.Width).IsRequired();
        builder.Property(p => p.Length).IsRequired();

        builder.ToTable("Products");
    }
}
