namespace MealPrepPlanner.Dal.Configurations.Recipes;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NpgsqlTypes;

/// <summary>
/// Maps <c>recipes</c>. <c>instructions</c> is <c>text[]</c>. <c>tags</c> and
/// <c>equipment_needed</c> are <c>text[]</c> (GIN). Full-text search uses a
/// generated <c>tsvector</c> column over <c>name</c> and <c>description</c>.
/// </summary>
public class RecipeConfiguration : IEntityTypeConfiguration<RecipeEntity>
{
    public void Configure(EntityTypeBuilder<RecipeEntity> builder)
    {
        builder.ToTable("recipes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Instructions)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(r => r.BaseServings)
            .HasDefaultValue(2);

        builder.Property(r => r.EquipmentNeeded)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(r => r.Tags)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(r => r.Source)
            .HasMaxLength(50);

        builder.Property(r => r.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.UseXminAsConcurrencyToken();

        builder.Property(r => r.PrepTimeMinutes).HasColumnName("prep_time_minutes");
        builder.Property(r => r.CookTimeMinutes).HasColumnName("cook_time_minutes");

        // Generated column `total_time_minutes = prep_time_minutes + cook_time_minutes`.
        builder.Property<int>("TotalTimeMinutes")
            .HasColumnName("total_time_minutes")
            .HasComputedColumnSql("\"prep_time_minutes\" + \"cook_time_minutes\"", stored: true);

        builder.HasIndex(r => r.Tags)
            .HasMethod("gin");

        builder.HasIndex(r => r.EquipmentNeeded)
            .HasMethod("gin");

        builder.HasOne<MealPrepPlanner.Dal.Entities.UserPreferences.HouseholdEntity>()
            .WithMany()
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Generated tsvector over name + description, with GIN index for FTS.
        var searchTsv = builder.Property<NpgsqlTypes.NpgsqlTsVector>("SearchTsv")
            .HasColumnName("search_tsv")
            .HasColumnType("tsvector")
            .ValueGeneratedOnAddOrUpdate()
            .IsGeneratedTsVectorColumn(
                "english",
                new[] { nameof(RecipeEntity.Name), nameof(RecipeEntity.Description) })
            .Metadata;

        searchTsv.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        searchTsv.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex("SearchTsv")
            .HasMethod("gin")
            .HasDatabaseName("ix_recipes_search_tsv_gin");
    }
}
