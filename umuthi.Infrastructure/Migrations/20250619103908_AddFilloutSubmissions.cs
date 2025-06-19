using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace umuthi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilloutSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilloutSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FormId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FormName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SubmissionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingAttempts = table.Column<int>(type: "int", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilloutSubmissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_CorrelationId",
                table: "FilloutSubmissions",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_CreatedAt",
                table: "FilloutSubmissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_FormId",
                table: "FilloutSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_ProcessingStatus",
                table: "FilloutSubmissions",
                column: "ProcessingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_SubmissionId",
                table: "FilloutSubmissions",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilloutSubmissions_SubmissionTime",
                table: "FilloutSubmissions",
                column: "SubmissionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilloutSubmissions");
        }
    }
}
