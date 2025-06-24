using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace umuthi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInitializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectInitializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GoogleSheetRowId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilloutData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MakeCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInitializations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CorrelationId",
                table: "ProjectInitializations",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CreatedAt",
                table: "ProjectInitializations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CustomerEmail",
                table: "ProjectInitializations",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_CustomerEmail_GoogleSheetRowId",
                table: "ProjectInitializations",
                columns: new[] { "CustomerEmail", "GoogleSheetRowId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInitializations_MakeCustomerId",
                table: "ProjectInitializations",
                column: "MakeCustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInitializations");
        }
    }
}
