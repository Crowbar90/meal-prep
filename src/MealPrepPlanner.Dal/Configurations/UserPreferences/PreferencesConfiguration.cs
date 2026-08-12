namespace MealPrepPlanner.Dal.Configurations.UserPreferences;

using System.Text.Json;

using MealPrepPlanner.Dal.Entities;
using MealPrepPlanner.Dal.Entities.UserPreferences;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps the consolidated household preferences row. One row per household.
/// Scalar lists (<c>dietary_restrictions</c>, <c>equipment</c>,
/// <c>preferred_supermarkets</c>) are stored as <c>text[]</c>; structured
/// documents (<c>nutrition_goals</c>, <c>food_preferences</c>) as JSONB.
/// Money is split into two scalar columns.
/// </summary>
public class PreferencesConfiguration : IEntityTypeConfiguration<PreferencesEntity>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<PreferencesEntity> builder)
    {
        builder.ToTable("preferences");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DietaryRestrictions)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(p => p.Equipment)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(p => p.MaxCookingTimeMinutes)
            .HasDefaultValue(60);

        builder.Property(p => p.WeeklyBudgetAmount)
            .HasColumnName("weekly_budget_amount")
            .HasColumnType("numeric(8,2)")
            .HasDefaultValue(0m);

        builder.Property(p => p.WeeklyBudgetCurrency)
            .HasColumnName("weekly_budget_currency")
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(p => p.PreferredSupermarkets)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(p => p.NutritionGoals)
            .HasColumnName("nutrition_goals")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<NutritionGoalsDocument>(v, JsonOpts) ?? new NutritionGoalsDocument())
            .IsRequired();

        builder.Property(p => p.FoodPreferences)
            .HasColumnName("food_preferences")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<FoodPreferencesDocument>(v, JsonOpts) ?? new FoodPreferencesDocument())
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        // One preferences row per household.
        builder.HasIndex(p => p.HouseholdId)
            .IsUnique();

        // GIN indexes per data-model.md for the two scalar-array JSONB-ish columns.
        builder.HasIndex(p => p.DietaryRestrictions)
            .HasMethod("gin");

        builder.HasIndex(p => p.FoodPreferences)
            .HasMethod("gin")
            .HasDatabaseName("ix_preferences_food_preferences_gin");
    }
}
