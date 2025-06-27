using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace umuthi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerEmailField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectInitializations_CustomerEmail",
                table: "ProjectInitializations");

            migrationBuilder.DropIndex(
                name: "IX_ProjectInitializations_CustomerEmail_GoogleSheetRowId",
                table: "ProjectInitializations");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "ProjectInitializations");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_GoogleSheetRowId",
                table: "ProjectInitializations",
                column: "GoogleSheetRowId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectInitializations_GoogleSheetRowId",
                table: "ProjectInitializations");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "ProjectInitializations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CustomerEmail",
                table: "ProjectInitializations",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CustomerEmail_GoogleSheetRowId",
                table: "ProjectInitializations",
                columns: new[] { "CustomerEmail", "GoogleSheetRowId" },
                unique: true);
        }
    }
}
