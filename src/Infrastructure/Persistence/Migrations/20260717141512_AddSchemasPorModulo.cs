using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoeurApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemasPorModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shopping");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "shopping_lists",
                newName: "shopping_lists",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "products",
                newName: "products",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "list_items",
                newName: "list_items",
                newSchema: "shopping");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                schema: "users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "shopping_lists",
                schema: "shopping",
                newName: "shopping_lists");

            migrationBuilder.RenameTable(
                name: "products",
                schema: "shopping",
                newName: "products");

            migrationBuilder.RenameTable(
                name: "list_items",
                schema: "shopping",
                newName: "list_items");
        }
    }
}
