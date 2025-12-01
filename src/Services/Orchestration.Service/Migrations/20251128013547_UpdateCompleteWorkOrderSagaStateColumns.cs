using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orchestration.Service.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompleteWorkOrderSagaStateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old columns
            migrationBuilder.DropColumn(
                name: "AssetUpdated",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            migrationBuilder.DropColumn(
                name: "TechnicianUpdated",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            migrationBuilder.DropColumn(
                name: "AssetUpdateTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            migrationBuilder.DropColumn(
                name: "TechnicianUpdateTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            // Add new columns
            migrationBuilder.AddColumn<bool>(
                name: "AssetCompleted",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TechnicianCompleted",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletionTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new columns
            migrationBuilder.DropColumn(
                name: "AssetCompleted",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            migrationBuilder.DropColumn(
                name: "TechnicianCompleted",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            migrationBuilder.DropColumn(
                name: "CompletionTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates");

            // Restore old columns
            migrationBuilder.AddColumn<bool>(
                name: "AssetUpdated",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TechnicianUpdated",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AssetUpdateTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TechnicianUpdateTimeoutTokenId",
                schema: "sagas",
                table: "CompleteWorkOrderSagaStates",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}

