namespace MealPrepPlanner.Dal.Configurations.Shopping;

using MealPrepPlanner.Dal.Entities.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps <c>shopping_lists</c>. Money is split into amount + currency columns.
/// </summary>
public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingListEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListEntity> builder)
    {
        builder.ToTable("shopping_lists");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.EstimatedTotalCost)
            .HasColumnName("estimated_total_cost")
            .HasColumnType("numeric(8,2)");

        builder.Property(l => l.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(l => l.Status)
            .HasMaxLength(20)
            .HasDefaultValue("pending")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasOne<MealPrepPlanner.Dal.Entities.MealPlanning.MealPlanEntity>()
            .WithMany()
            .HasForeignKey(l => l.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SupermarketEntity>()
            .WithMany()
            .HasForeignKey(l => l.SupermarketId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
