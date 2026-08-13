namespace MealPrepPlanner.Dal.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Shared helper that maps the Postgres <c>xmin</c> system column as an
/// optimistic concurrency token on an aggregate root. EF Core 10's Npgsql
/// provider detects a <c>uint</c> shadow property configured as a row version
/// and maps it to the implicit <c>xmin</c> column — no application code needs
/// to read or write it.
/// </summary>
public static class XminConcurrencyExtensions
{
    public static void UseXminAsConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        var prop = builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken()
            .Metadata;

        prop.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        prop.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
