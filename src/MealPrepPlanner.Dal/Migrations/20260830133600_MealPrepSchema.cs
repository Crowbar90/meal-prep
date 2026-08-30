using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrepPlanner.Dal.Migrations
{
    /// <inheritdoc />
    public partial class MealPrepSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prep_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meal_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    workflow_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    feasibility_violations = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prep_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_prep_schedules_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prep_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prep_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    meal_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    batch_size_servings = table.Column<int>(type: "integer", nullable: false),
                    earliest_start_offset_minutes = table.Column<int>(type: "integer", nullable: true),
                    latest_finish_offset_minutes = table.Column<int>(type: "integer", nullable: true),
                    equipment_ids = table.Column<string[]>(type: "text[]", nullable: false),
                    steps = table.Column<string>(type: "jsonb", nullable: false),
                    assigned_to_slot_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prep_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_prep_tasks_prep_schedules_prep_schedule_id",
                        column: x => x.prep_schedule_id,
                        principalTable: "prep_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_prep_tasks_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prep_schedules_household_id_week_start_date",
                table: "prep_schedules",
                columns: new[] { "household_id", "week_start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_prep_schedules_meal_plan_id",
                table: "prep_schedules",
                column: "meal_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_prep_schedules_status",
                table: "prep_schedules",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_prep_tasks_prep_schedule_id_day_of_week_meal_type",
                table: "prep_tasks",
                columns: new[] { "prep_schedule_id", "day_of_week", "meal_type" });

            migrationBuilder.CreateIndex(
                name: "ix_prep_tasks_recipe_id",
                table: "prep_tasks",
                column: "recipe_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prep_tasks");

            migrationBuilder.DropTable(
                name: "prep_schedules");
        }
    }
}
