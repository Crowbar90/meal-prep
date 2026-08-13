namespace MealPrepPlanner.Dal.Configurations.UserPreferences;

using MealPrepPlanner.Dal.Entities.UserPreferences;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HouseholdMemberConfiguration : IEntityTypeConfiguration<HouseholdMemberEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdMemberEntity> builder)
    {
        builder.ToTable("household_members");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Sex)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.ActivityLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.WeightKg)
            .HasColumnType("numeric(5,2)");

        builder.Property(m => m.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        // Per data-model.md: `age > 0`.
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_household_members_age_positive",
            "\"age\" > 0"));

        builder.HasIndex(m => new { m.HouseholdId, m.Name })
            .IsUnique();
    }
}
