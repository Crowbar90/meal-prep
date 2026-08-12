namespace MealPrepPlanner.Dal.Configurations.Recipes;

using System.Text.Json;

using MealPrepPlanner.Dal.Configurations;
using MealPrepPlanner.Dal.Entities;
using MealPrepPlanner.Dal.Entities.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>ingredients</c>. <c>nutrition_per_100g</c> is JSONB; <c>allergens</c>
/// is <c>text[]</c> for cheap set-membership queries via GIN. A generated
/// <c>tsvector</c> column backs the full-text search index on <c>name</c>.
/// </summary>
public class IngredientConfiguration : IEntityTypeConfiguration<IngredientEntity>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<IngredientEntity> builder)
    {
        builder.ToTable("ingredients");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Category)
            .HasMaxLength(50);

        builder.Property(i => i.DefaultUnit)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Allergens)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .IsRequired();

        builder.Property(i => i.NutritionPer100g)
            .HasColumnName("nutrition_per_100g")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<NutritionProfileDocument>(v, JsonOpts) ?? new NutritionProfileDocument())
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.UseXminAsConcurrencyToken();

        // GIN on allergens for set-membership queries.
        builder.HasIndex(i => i.Allergens)
            .HasMethod("gin");

        // Generated tsvector column over `name` + GIN index for full-text search.
        var nameTsv = builder.Property<NpgsqlTypes.NpgsqlTsVector>("NameTsv")
            .HasColumnName("name_tsv")
            .HasColumnType("tsvector")
            .ValueGeneratedOnAddOrUpdate()
            .IsGeneratedTsVectorColumn(
                "english",
                new[] { nameof(IngredientEntity.Name) })
            .Metadata;

        nameTsv.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        nameTsv.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex("NameTsv")
            .HasMethod("gin")
            .HasDatabaseName("ix_ingredients_name_tsv_gin");
    }
}
