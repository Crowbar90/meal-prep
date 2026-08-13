namespace MealPrepPlanner.Dal.Configurations.Audit;

using MealPrepPlanner.Dal.Entities.Audit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Maps the append-only <c>decision_events</c> table.
/// <c>id</c> is <c>BIGSERIAL</c> because all writes funnel into a single
/// Postgres sequence even with multiple backend replicas (one writer of state
/// — the DB itself). <c>sequence_number</c> is supplied by the caller
/// (OpenClaw owns workflow ordering per ADR 005).
/// </summary>
public class DecisionEventConfiguration : IEntityTypeConfiguration<DecisionEventEntity>
{
    public void Configure(EntityTypeBuilder<DecisionEventEntity> builder)
    {
        builder.ToTable("decision_events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.Timestamp)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(e => e.ActorType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ActorName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.DecisionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.InputContext)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(e => e.OutputDecision)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(e => e.Reasoning)
            .HasColumnType("text");

        // Append-only: no concurrency token, no updated_at, no xmin. We never UPDATE this row.

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_decision_events_actor_type",
            "\"actor_type\" IN ('USER','AI_AGENT','BACKEND_SERVICE')"));

        builder.HasIndex(e => new { e.WorkflowId, e.SequenceNumber })
            .IsUnique();

        builder.HasIndex(e => e.WorkflowId);

        builder.HasIndex(e => new { e.ActorName, e.Timestamp });

        builder.HasIndex(e => e.InputContext)
            .HasMethod("gin");

        builder.HasIndex(e => e.OutputDecision)
            .HasMethod("gin");

        builder.HasOne<DecisionEventEntity>()
            .WithMany()
            .HasForeignKey(e => e.ParentDecisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
