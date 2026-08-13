namespace MealPrepPlanner.Dal.Configurations.Shopping;

using MealPrepPlanner.Dal.Entities.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>supermarket_prices</c>. Append-mostly: a new row is written when a
/// price is observed, and the current price is the row with the latest
/// <c>recorded_at</c> per (supermarket, ingredient) pair.
/// </summary>
public class SupermarketPriceConfiguration : IEntityTypeConfiguration<SupermarketPriceEntity>
{
    public void Configure(EntityTypeBuilder<SupermarketPriceEntity> builder)
    {
        builder.ToTable("supermarket_prices");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Price)
            .HasColumnType("numeric(6,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(p => p.PackageSize)
            .HasColumnType("numeric(10,3)");

        builder.Property(p => p.PackageUnit)
            .HasMaxLength(20);

        builder.Property(p => p.RecordedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasOne<SupermarketEntity>()
            .WithMany()
            .HasForeignKey(p => p.SupermarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MealPrepPlanner.Dal.Entities.Recipes.IngredientEntity>()
            .WithMany()
            .HasForeignKey(p => p.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Per data-model.md: composite index, descending on recorded_at.
        builder.HasIndex(p => new { p.SupermarketId, p.IngredientId, p.RecordedAt })
            .IsDescending(false, false, true);
    }
}
