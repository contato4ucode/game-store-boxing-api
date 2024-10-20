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

        builder.Property(b => b.Height).IsRequired();
        builder.Property(b => b.Width).IsRequired();
        builder.Property(b => b.Length).IsRequired();

        builder.Ignore(b => b.Volume);

        builder.HasIndex(b => b.Name).HasDatabaseName("IX_Boxes_Name");

        builder.ToTable("Boxes");

        builder.HasData(
            new Box("Box 1", 30, 40, 80)
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUser = "System"
            },
            new Box("Box 2", 80, 50, 40)
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUser = "System"
            },
            new Box("Box 3", 50, 80, 60)
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUser = "System"
            }
        );
    }
}
