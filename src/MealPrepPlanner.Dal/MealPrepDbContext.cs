namespace MealPrepPlanner.Dal;

using MealPrepPlanner.Dal.Entities.Audit;
using MealPrepPlanner.Dal.Entities.MealPlanning;
using MealPrepPlanner.Dal.Entities.Pantry;
using MealPrepPlanner.Dal.Entities.Recipes;
using MealPrepPlanner.Dal.Entities.Shopping;
using MealPrepPlanner.Dal.Entities.UserPreferences;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Single database context spanning every bounded context. Per
/// <c>docs/architecture/bounded-contexts.md</c> the contexts share one schema in
/// the monolithic backend; splitting the context per bounded context would force
/// cross-context navigation gymnastics and is reserved for a future microservices
/// split.
/// </summary>
public class MealPrepDbContext : DbContext
{
    public MealPrepDbContext(DbContextOptions<MealPrepDbContext> options)
        : base(options)
    {
    }

    // User Preferences
    public DbSet<HouseholdEntity> Households => Set<HouseholdEntity>();

    public DbSet<HouseholdMemberEntity> HouseholdMembers => Set<HouseholdMemberEntity>();

    public DbSet<PreferencesEntity> Preferences => Set<PreferencesEntity>();

    // Recipes
    public DbSet<IngredientEntity> Ingredients => Set<IngredientEntity>();

    public DbSet<RecipeEntity> Recipes => Set<RecipeEntity>();

    public DbSet<RecipeIngredientEntity> RecipeIngredients => Set<RecipeIngredientEntity>();

    // Meal Planning
    public DbSet<MealPlanEntity> MealPlans => Set<MealPlanEntity>();

    public DbSet<MealSlotEntity> MealSlots => Set<MealSlotEntity>();

    // Pantry
    public DbSet<PantryItemEntity> PantryItems => Set<PantryItemEntity>();

    // Shopping
    public DbSet<ShoppingListEntity> ShoppingLists => Set<ShoppingListEntity>();

    public DbSet<ShoppingListItemEntity> ShoppingListItems => Set<ShoppingListItemEntity>();

    public DbSet<SupermarketEntity> Supermarkets => Set<SupermarketEntity>();

    public DbSet<SupermarketPriceEntity> SupermarketPrices => Set<SupermarketPriceEntity>();

    // Audit
    public DbSet<DecisionEventEntity> DecisionEvents => Set<DecisionEventEntity>();

    public DbSet<AiExecutionLogEntity> AiExecutionLogs => Set<AiExecutionLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MealPrepDbContext).Assembly);
    }
}
