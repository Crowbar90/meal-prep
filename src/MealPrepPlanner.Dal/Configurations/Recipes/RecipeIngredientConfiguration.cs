namespace MealPrepPlanner.Dal.Configurations.Recipes;

using MealPrepPlanner.Dal.Entities.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>recipe_ingredients</c>. The Domain's <c>RecipeIngredient</c> is a
/// value object carrying an ingredient snapshot; the DAL breaks that out into
/// scalar columns plus the FK to the ingredient.
/// </summary>
public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredientEntity>
{
    public void Configure(EntityTypeBuilder<RecipeIngredientEntity> builder)
    {
        builder.ToTable("recipe_ingredients");
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.QuantityAmount)
            .HasColumnName("quantity")
            .HasColumnType("numeric(10,3)")
            .IsRequired();

        builder.Property(ri => ri.QuantityUnit)
            .HasColumnName("unit")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ri => ri.Preparation)
            .HasMaxLength(100);

        builder.Property(ri => ri.IngredientName)
            .HasColumnName("ingredient_name_snapshot")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne<IngredientEntity>()
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
