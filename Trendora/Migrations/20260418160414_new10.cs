using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trendora.Migrations
{
    /// <inheritdoc />
    public partial class new10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the necessary migration logic here
            migrationBuilder.AddColumn<string>(
                name: "NewColumn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback logic for the migration
            migrationBuilder.DropColumn(
                name: "NewColumn",
                table: "Products");
        }
    }
}
