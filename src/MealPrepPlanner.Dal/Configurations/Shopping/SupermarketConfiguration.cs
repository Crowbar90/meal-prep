namespace MealPrepPlanner.Dal.Configurations.Shopping;

using MealPrepPlanner.Dal.Entities.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SupermarketConfiguration : IEntityTypeConfiguration<SupermarketEntity>
{
    public void Configure(EntityTypeBuilder<SupermarketEntity> builder)
    {
        builder.ToTable("supermarkets");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Chain)
            .HasMaxLength(50);

        builder.Property(s => s.Location)
            .HasMaxLength(200);

        builder.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");
    }
}
