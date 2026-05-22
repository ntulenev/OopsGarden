using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WateringEvents_PlantId",
                table: "WateringEvents");

            migrationBuilder.DropIndex(
                name: "IX_Plants_UserId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_PlantNotes_PlantId_CreatedAt",
                table: "PlantNotes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_UserId",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_WateringEvents_PlantId_WateredAt",
                table: "WateringEvents",
                columns: ["PlantId", "WateredAt"]);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_UserId_LocationId",
                table: "Plants",
                columns: ["UserId", "LocationId"]);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_UserId_Name",
                table: "Plants",
                columns: ["UserId", "Name"]);

            migrationBuilder.CreateIndex(
                name: "IX_PlantNotes_PlantId_CreatedAt_Id",
                table: "PlantNotes",
                columns: ["PlantId", "CreatedAt", "Id"]);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_UserId_Name",
                table: "Locations",
                columns: ["UserId", "Name"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WateringEvents_PlantId_WateredAt",
                table: "WateringEvents");

            migrationBuilder.DropIndex(
                name: "IX_Plants_UserId_LocationId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_UserId_Name",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_PlantNotes_PlantId_CreatedAt_Id",
                table: "PlantNotes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_UserId_Name",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_WateringEvents_PlantId",
                table: "WateringEvents",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_UserId",
                table: "Plants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantNotes_PlantId_CreatedAt",
                table: "PlantNotes",
                columns: ["PlantId", "CreatedAt"]);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_UserId",
                table: "Locations",
                column: "UserId");
        }
    }
}
