using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantPhotoHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotoDataUrl = table.Column<string>(type: "nvarchar(max)", maxLength: 1500000, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantPhotos_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [PlantPhotos] ([Id], [PlantId], [PhotoDataUrl], [UploadedAt])
                SELECT NEWID(), [Id], [PhotoDataUrl], [CreatedAt]
                FROM [Plants]
                WHERE [PhotoDataUrl] IS NOT NULL AND [PhotoDataUrl] <> N''
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PlantPhotos_PlantId_UploadedAt_Id",
                table: "PlantPhotos",
                columns: ["PlantId", "UploadedAt", "Id"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantPhotos");
        }
    }
}
