using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoeurApi.Modules.Shopping.Domain;

namespace CoeurApi.Modules.Shopping.Infrastructure.Persistence.Configurations;

public class ListItemConfiguration : IEntityTypeConfiguration<ListItem>
{
    public void Configure(EntityTypeBuilder<ListItem> builder)
    {
        builder.ToTable("list_items", "shopping");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(i => i.Unit)
            .HasMaxLength(20);

        builder.Property(i => i.IsChecked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
