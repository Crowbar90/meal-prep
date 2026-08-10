namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// Base class for all entities. Entities have identity and can be mutated,
/// but state changes must be driven through the owning aggregate root.
/// </summary>
public abstract class Entity
{
    private protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity id must not be empty.", nameof(id));

        Id = id;
    }

    public Guid Id { get; private protected set; }
}
