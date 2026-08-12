namespace MealPrepPlanner.Dal.Configurations.Shopping;

using MealPrepPlanner.Dal.Entities.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>shopping_list_items</c>. Quantities and money values are split into
/// paired (amount, unit/currency) scalar columns.
/// </summary>
public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItemEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListItemEntity> builder)
    {
        builder.ToTable("shopping_list_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantityNeededAmount)
            .HasColumnName("quantity_needed")
            .HasColumnType("numeric(10,3)");

        builder.Property(i => i.QuantityNeededUnit)
            .HasColumnName("quantity_needed_unit")
            .HasMaxLength(20);

        builder.Property(i => i.QuantityToBuyAmount)
            .HasColumnName("quantity_to_buy")
            .HasColumnType("numeric(10,3)");

        builder.Property(i => i.QuantityToBuyUnit)
            .HasColumnName("quantity_to_buy_unit")
            .HasMaxLength(20);

        builder.Property(i => i.EstimatedPriceAmount)
            .HasColumnName("estimated_price")
            .HasColumnType("numeric(6,2)");

        builder.Property(i => i.EstimatedPriceCurrency)
            .HasColumnName("estimated_price_currency")
            .HasMaxLength(3);

        builder.Property(i => i.Purchased)
            .HasDefaultValue(false);

        builder.Property(i => i.PriceAtPurchaseAmount)
            .HasColumnName("price_at_purchase")
            .HasColumnType("numeric(6,2)");

        builder.Property(i => i.PriceAtPurchaseCurrency)
            .HasColumnName("price_at_purchase_currency")
            .HasMaxLength(3);

        builder.Property(i => i.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasOne<MealPrepPlanner.Dal.Entities.Recipes.IngredientEntity>()
            .WithMany()
            .HasForeignKey(i => i.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
