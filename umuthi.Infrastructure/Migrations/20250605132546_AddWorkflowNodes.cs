using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace umuthi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NodeTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NodeType = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigurationSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeType = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNodes_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourcePort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowConnections_WorkflowNodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowConnections_WorkflowNodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowConnections_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_Category",
                table: "NodeTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_IsActive",
                table: "NodeTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_ModuleType",
                table: "NodeTemplates",
                column: "ModuleType");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_Name",
                table: "NodeTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_NodeType",
                table: "NodeTemplates",
                column: "NodeType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_SourceNodeId",
                table: "WorkflowConnections",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_SourceNodeId_TargetNodeId_SourcePort_TargetPort",
                table: "WorkflowConnections",
                columns: new[] { "SourceNodeId", "TargetNodeId", "SourcePort", "TargetPort" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_TargetNodeId",
                table: "WorkflowConnections",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_WorkflowId",
                table: "WorkflowConnections",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_ModuleType",
                table: "WorkflowNodes",
                column: "ModuleType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_NodeType",
                table: "WorkflowNodes",
                column: "NodeType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_WorkflowId",
                table: "WorkflowNodes",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeTemplates");

            migrationBuilder.DropTable(
                name: "WorkflowConnections");

            migrationBuilder.DropTable(
                name: "WorkflowNodes");
        }
    }
}
