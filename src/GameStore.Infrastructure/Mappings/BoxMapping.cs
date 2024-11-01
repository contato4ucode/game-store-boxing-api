using GameStore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Mappings;

public class BoxMapping : EntityBaseMapping<Box>
{
    public override void Configure(EntityTypeBuilder<Box> builder)
    {
        base.Configure(builder);

        builder.Property(b => b.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.OwnsOne(b => b.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Height)
                .IsRequired()
                .HasColumnName("Height");

            dimensions.Property(d => d.Width)
                .IsRequired()
                .HasColumnName("Width");

            dimensions.Property(d => d.Length)
                .IsRequired()
                .HasColumnName("Length");
        });

        builder.Ignore(b => b.Volume);

        builder.HasIndex(b => b.Name).HasDatabaseName("IX_Boxes_Name");

        builder.ToTable("Boxes");
    }
}
