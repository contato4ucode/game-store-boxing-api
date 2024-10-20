using GameStore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Mappings;

public class ProductMapping : EntityBaseMapping<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Height)
            .IsRequired();

        builder.Property(p => p.Width)
            .IsRequired();

        builder.Property(p => p.Length)
            .IsRequired();

        builder.Property(p => p.Weight)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasIndex(p => p.Name).HasDatabaseName("IX_Products_Name");

        builder.ToTable("Products");
    }
}
