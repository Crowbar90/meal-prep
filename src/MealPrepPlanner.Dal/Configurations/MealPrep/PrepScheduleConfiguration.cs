namespace MealPrepPlanner.Dal.Configurations.MealPrep;

using System.Text.Json;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities;
using MealPrepPlanner.Dal.Entities.MealPrep;
using MealPrepPlanner.Dal.Entities.UserPreferences;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>prep_schedules</c>. <c>feasibility_violations</c> is JSONB.
/// Domain enums (<see cref="MealPrepPlanner.Domain.MealPrep.PrepScheduleStatus"/>)
/// are serialized to canonical lower-case names by the Application layer; the
/// DAL stores the resulting <c>string</c> directly.
/// </summary>
public class PrepScheduleConfiguration : IEntityTypeConfiguration<PrepScheduleEntity>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<PrepScheduleEntity> builder)
    {
        builder.ToTable("prep_schedules");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.WeekStartDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .HasDefaultValue("draft")
            .IsRequired();

        builder.Property(p => p.Version)
            .HasDefaultValue(1);

        builder.Property(p => p.FeasibilityViolations)
            .HasColumnName("feasibility_violations")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<PrepFeasibilityViolationsRow>(v, JsonOpts));

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

        builder.HasMany(p => p.Tasks)
            .WithOne()
            .HasForeignKey(t => t.PrepScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.HouseholdId, p.WeekStartDate });
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.MealPlanId);
    }
}
