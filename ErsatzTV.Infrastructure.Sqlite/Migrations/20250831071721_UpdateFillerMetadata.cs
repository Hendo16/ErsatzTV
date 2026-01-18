using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErsatzTV.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFillerMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Director_FillerMetadata_FillerMetadataId",
                table: "Director");

            migrationBuilder.DropForeignKey(
                name: "FK_Writer_FillerMetadata_FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "FillerMetadata");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Director");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "FillerMetadata",
                newName: "ContentRating");

            migrationBuilder.AddColumn<int>(
                name: "MovieId",
                table: "FillerMetadata",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShowId",
                table: "FillerMetadata",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovieId",
                table: "FillerMetadata");

            migrationBuilder.DropColumn(
                name: "ShowId",
                table: "FillerMetadata");

            migrationBuilder.RenameColumn(
                name: "ContentRating",
                table: "FillerMetadata",
                newName: "Country");

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Writer",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "FillerMetadata",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Director",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Writer_FillerMetadataId",
                table: "Writer",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Director_FillerMetadataId",
                table: "Director",
                column: "FillerMetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Director_FillerMetadata_FillerMetadataId",
                table: "Director",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Writer_FillerMetadata_FillerMetadataId",
                table: "Writer",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
