using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantNoteReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReminder",
                table: "PlantNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReminderResolved",
                table: "PlantNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReminderDate",
                table: "PlantNotes",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantNotes_PlantId_IsReminder_IsReminderResolved_ReminderDate",
                table: "PlantNotes",
                columns: ["PlantId", "IsReminder", "IsReminderResolved", "ReminderDate"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlantNotes_PlantId_IsReminder_IsReminderResolved_ReminderDate",
                table: "PlantNotes");

            migrationBuilder.DropColumn(
                name: "IsReminder",
                table: "PlantNotes");

            migrationBuilder.DropColumn(
                name: "IsReminderResolved",
                table: "PlantNotes");

            migrationBuilder.DropColumn(
                name: "ReminderDate",
                table: "PlantNotes");
        }
    }
}
