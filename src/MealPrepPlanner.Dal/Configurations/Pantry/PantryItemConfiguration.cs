namespace MealPrepPlanner.Dal.Configurations.Pantry;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities.Pantry;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>pantry_items</c>. Quantity is split into two scalar columns
/// (<c>quantity_amount</c>, <c>quantity_unit</c>).
/// </summary>
public class PantryItemConfiguration : IEntityTypeConfiguration<PantryItemEntity>
{
    public void Configure(EntityTypeBuilder<PantryItemEntity> builder)
    {
        builder.ToTable("pantry_items");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.QuantityAmount)
            .HasColumnName("quantity")
            .HasColumnType("numeric(10,3)")
            .IsRequired();

        builder.Property(p => p.QuantityUnit)
            .HasColumnName("unit")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.DateAdded)
            .HasColumnType("date")
            .HasDefaultValueSql("current_date");

        builder.Property(p => p.ExpiresAt)
            .HasColumnType("date");

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .HasDefaultValue("available")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.UseXminAsConcurrencyToken();

        builder.HasOne<MealPrepPlanner.Dal.Entities.Recipes.IngredientEntity>()
            .WithMany()
            .HasForeignKey(p => p.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MealPrepPlanner.Dal.Entities.UserPreferences.HouseholdEntity>()
            .WithMany()
            .HasForeignKey(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.HouseholdId, p.Status });
        builder.HasIndex(p => p.ExpiresAt);
    }
}
