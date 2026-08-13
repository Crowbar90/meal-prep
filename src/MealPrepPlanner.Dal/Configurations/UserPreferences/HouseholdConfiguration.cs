namespace MealPrepPlanner.Dal.Configurations.UserPreferences;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities.UserPreferences;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps the household aggregate root. Snake_case naming + xmin concurrency
/// token are inherited from conventions; this config pins table-level
/// constraints, the FK to <c>preferences</c>, and the cascade behavior.
/// </summary>
public class HouseholdConfiguration : IEntityTypeConfiguration<HouseholdEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdEntity> builder)
    {
        builder.ToTable("households");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id");

        builder.Property(h => h.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(h => h.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.UseXminAsConcurrencyToken();

        builder.HasMany(h => h.Members)
            .WithOne()
            .HasForeignKey(m => m.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Preferences)
            .WithOne()
            .HasForeignKey<PreferencesEntity>(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
