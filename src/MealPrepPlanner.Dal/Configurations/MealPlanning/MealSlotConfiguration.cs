namespace MealPrepPlanner.Dal.Configurations.MealPlanning;

using MealPrepPlanner.Dal.Entities.MealPlanning;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>meal_slots</c>. The Domain's enums (<see cref="DayOfWeek"/>,
/// <see cref="MealPrepPlanner.Domain.MealPlanning.MealType"/>) are serialized
/// to canonical lower-case names by the Application layer; the DAL stores the
/// resulting <c>string</c> directly. <c>day_of_week</c> is e.g. "monday",
/// <c>meal_type</c> is e.g. "breakfast".
/// </summary>
public class MealSlotConfiguration : IEntityTypeConfiguration<MealSlotEntity>
{
    public void Configure(EntityTypeBuilder<MealSlotEntity> builder)
    {
        builder.ToTable("meal_slots");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.MealType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.RecipeName)
            .HasMaxLength(200);

        builder.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasOne<MealPrepPlanner.Dal.Entities.Recipes.RecipeEntity>()
            .WithMany()
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.MealPlanId, s.DayOfWeek, s.MealType });
    }
}
