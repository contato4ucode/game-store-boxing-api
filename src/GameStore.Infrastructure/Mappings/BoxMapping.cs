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

        builder.ToTable("Boxes");
    }
}
