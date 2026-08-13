namespace MealPrepPlanner.Dal.Configurations.Audit;

using MealPrepPlanner.Dal.Entities.Audit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps the append-only <c>ai_execution_logs</c> table. Stores raw prompts
/// and responses for debugging agent behavior; never updated.
/// </summary>
public class AiExecutionLogConfiguration : IEntityTypeConfiguration<AiExecutionLogEntity>
{
    public void Configure(EntityTypeBuilder<AiExecutionLogEntity> builder)
    {
        builder.ToTable("ai_execution_logs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(l => l.AgentName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Prompt)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(l => l.Response)
            .HasColumnType("text");

        builder.Property(l => l.Model)
            .HasMaxLength(50);

        builder.Property(l => l.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.HasIndex(l => l.WorkflowId);
        builder.HasIndex(l => new { l.AgentName, l.CreatedAt });
    }
}
