using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orchestration.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialSagaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sagas");

            migrationBuilder.CreateTable(
                name: "AssignTechnicianSagaStates",
                schema: "sagas",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignTechnicianSagaStates", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "CompleteWorkOrderSagaStates",
                schema: "sagas",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetUpdated = table.Column<bool>(type: "bit", nullable: false),
                    TechnicianUpdated = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetUpdateTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicianUpdateTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompleteWorkOrderSagaStates", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignTechnicianSagaStates",
                schema: "sagas");

            migrationBuilder.DropTable(
                name: "CompleteWorkOrderSagaStates",
                schema: "sagas");
        }
    }
}
