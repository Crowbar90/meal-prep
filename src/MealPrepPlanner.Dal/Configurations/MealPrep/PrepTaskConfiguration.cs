namespace MealPrepPlanner.Dal.Configurations.MealPrep;

using System.Text.Json;

using MealPrepPlanner.Dal.Entities.MealPrep;
using MealPrepPlanner.Dal.Entities.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>prep_tasks</c>. <c>steps</c> is JSONB. Domain enums
/// (<c>DayOfWeek</c>, <see cref="MealPrepPlanner.Domain.MealPlanning.MealType"/>)
/// are serialized to canonical lower-case names by the Application layer; the
/// DAL stores the resulting <c>string</c> directly.
/// </summary>
public class PrepTaskConfiguration : IEntityTypeConfiguration<PrepTaskEntity>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<PrepTaskEntity> builder)
    {
        builder.ToTable("prep_tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.DayOfWeek)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.MealType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.BatchSizeServings)
            .IsRequired();

        builder.Property(t => t.EquipmentIds)
            .HasColumnType("text[]");

        builder.Property(t => t.AssignedToSlotIds)
            .HasColumnType("uuid[]");

        builder.Property(t => t.Steps)
            .HasColumnName("steps")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<PrepTaskStepRow>>(v, JsonOpts) ?? new List<PrepTaskStepRow>());

        builder.Property(t => t.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasOne<RecipeEntity>()
            .WithMany()
            .HasForeignKey(t => t.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.PrepScheduleId, t.DayOfWeek, t.MealType });
        builder.HasIndex(t => t.RecipeId);
    }
}
