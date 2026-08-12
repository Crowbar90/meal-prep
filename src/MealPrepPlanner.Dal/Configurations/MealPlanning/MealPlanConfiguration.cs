namespace MealPrepPlanner.Dal.Configurations.MealPlanning;

using System.Text.Json;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities;
using MealPrepPlanner.Dal.Entities.MealPlanning;
using MealPrepPlanner.Dal.Entities.UserPreferences;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>meal_plans</c>. <c>nutrition_summary</c> is JSONB.
/// </summary>
public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlanEntity>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<MealPlanEntity> builder)
    {
        builder.ToTable("meal_plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.WeekStartDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .HasDefaultValue("draft")
            .IsRequired();

        builder.Property(p => p.TotalEstimatedCost)
            .HasColumnType("numeric(8,2)");

        builder.Property(p => p.Version)
            .HasDefaultValue(1);

        builder.Property(p => p.NutritionSummary)
            .HasColumnName("nutrition_summary")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<MealPlanNutritionSummaryDocument>(v, JsonOpts));

        builder.Property(p => p.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.UseXminAsConcurrencyToken();

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Slots)
            .WithOne()
            .HasForeignKey(s => s.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.HouseholdId, p.WeekStartDate });
        builder.HasIndex(p => p.Status);
    }
}
