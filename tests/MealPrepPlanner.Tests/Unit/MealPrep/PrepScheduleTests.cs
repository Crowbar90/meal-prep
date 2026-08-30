namespace MealPrepPlanner.Tests.Unit.MealPrep;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.MealPrep;
using MealPrepPlanner.Domain.MealPrep.Events;

public class PrepScheduleTests
{
    private static readonly Guid HouseholdId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MealPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateOnly WeekStart = new(2026, 8, 17);

    [Fact]
    public void CreateDraft_SetsFieldsAndEmitsEvent()
    {
        var correlationId = Guid.NewGuid();

        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart, correlationId);

        Assert.NotEqual(Guid.Empty, schedule.Id);
        Assert.Equal(HouseholdId, schedule.HouseholdId);
        Assert.Equal(MealPlanId, schedule.MealPlanId);
        Assert.Equal(WeekStart, schedule.WeekStartDate);
        Assert.Equal(PrepScheduleStatus.Draft, schedule.Status);
        Assert.Equal(1, schedule.Version);
        Assert.Empty(schedule.Tasks);
        Assert.Null(schedule.WorkflowId);
        Assert.Null(schedule.FeasibilityViolations);

        var evt = Assert.Single(schedule.DomainEvents.OfType<PrepScheduleDraftCreated>());
        Assert.Equal(schedule.Id, evt.PrepScheduleId);
        Assert.Equal(HouseholdId, evt.HouseholdId);
        Assert.Equal(MealPlanId, evt.MealPlanId);
        Assert.Equal(WeekStart, evt.WeekStartDate);
        Assert.Equal(correlationId, evt.CorrelationId);
    }

    [Fact]
    public void CreateDraft_EmptyHouseholdId_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrepSchedule.CreateDraft(Guid.Empty, MealPlanId, WeekStart));
    }

    [Fact]
    public void CreateDraft_EmptyMealPlanId_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrepSchedule.CreateDraft(HouseholdId, Guid.Empty, WeekStart));
    }

    [Fact]
    public void AddTask_AppendsToTasks_AndBumpsUpdatedAt()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        var initialUpdatedAt = schedule.UpdatedAt;

        var task = schedule.AddTask(BuildTask(DayOfWeek.Sunday, MealType.Dinner));

        Assert.Single(schedule.Tasks);
        Assert.Same(task, schedule.Tasks[0]);
        Assert.True(schedule.UpdatedAt >= initialUpdatedAt);
    }

    [Fact]
    public void AssignWorkflow_StoresWorkflowId()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        var workflowId = Guid.NewGuid();

        schedule.AssignWorkflow(workflowId);

        Assert.Equal(workflowId, schedule.WorkflowId);
    }

    [Fact]
    public void AssignWorkflow_EmptyWorkflowId_Throws()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        Assert.Throws<ArgumentException>(() => schedule.AssignWorkflow(Guid.Empty));
    }

    [Fact]
    public void MarkFeasibilityChecked_TransitionsStatus_BumpsVersion_AndStoresViolations()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        var violations = new PrepFeasibilityViolationsDocument(
            DateTimeOffset.UtcNow,
            []);

        schedule.MarkFeasibilityChecked(violations);

        Assert.Equal(PrepScheduleStatus.FeasibilityChecked, schedule.Status);
        Assert.Equal(2, schedule.Version);
        Assert.Same(violations, schedule.FeasibilityViolations);
    }

    [Fact]
    public void MarkFeasibilityChecked_OnFinalizedSchedule_Throws()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        schedule.MarkFeasibilityChecked(new PrepFeasibilityViolationsDocument(DateTimeOffset.UtcNow, []));
        schedule.Finalize();

        Assert.Throws<InvalidOperationException>(() => schedule.MarkFeasibilityChecked(
            new PrepFeasibilityViolationsDocument(DateTimeOffset.UtcNow, [])));
    }

    [Fact]
    public void Finalize_OnDraft_WithoutFeasibilityCheck_Throws()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);

        Assert.Throws<InvalidOperationException>(() => schedule.Finalize());
    }

    [Fact]
    public void Finalize_WithOpenViolations_Throws()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        var violations = new PrepFeasibilityViolationsDocument(
            DateTimeOffset.UtcNow,
            [new PrepFeasibilityViolationDocument(PrepViolationKind.EquipmentUnavailable, Guid.NewGuid(), Guid.NewGuid(), "missing oven")]);
        schedule.MarkFeasibilityChecked(violations);

        Assert.Throws<InvalidOperationException>(() => schedule.Finalize());
    }

    [Fact]
    public void Finalize_AfterCleanFeasibilityCheck_TransitionsToFinalized()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        schedule.MarkFeasibilityChecked(new PrepFeasibilityViolationsDocument(DateTimeOffset.UtcNow, []));

        schedule.Finalize();

        Assert.Equal(PrepScheduleStatus.Finalized, schedule.Status);
        Assert.Contains(schedule.DomainEvents, e => e is PrepScheduleFinalized);
    }

    [Fact]
    public void Archive_FromFinalized_TransitionsToArchived()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        schedule.MarkFeasibilityChecked(new PrepFeasibilityViolationsDocument(DateTimeOffset.UtcNow, []));
        schedule.Finalize();

        schedule.Archive();

        Assert.Equal(PrepScheduleStatus.Archived, schedule.Status);
        Assert.Contains(schedule.DomainEvents, e => e is PrepScheduleArchived);
    }

    [Fact]
    public void Archive_AlreadyArchived_Throws()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        schedule.MarkFeasibilityChecked(new PrepFeasibilityViolationsDocument(DateTimeOffset.UtcNow, []));
        schedule.Finalize();
        schedule.Archive();

        Assert.Throws<InvalidOperationException>(() => schedule.Archive());
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventList()
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);

        schedule.ClearDomainEvents();

        Assert.Empty(schedule.DomainEvents);
    }

    private static PrepTask BuildTask(DayOfWeek day, MealType mealType) =>
        PrepTask.Create(
            Guid.NewGuid(),
            day,
            mealType,
            batchSizeServings: 4,
            earliestStartOffsetMinutes: 0,
            latestFinishOffsetMinutes: 60);
}
