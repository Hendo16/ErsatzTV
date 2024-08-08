using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErsatzTV.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Add_FillerMediaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Writer",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Tag",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Subtitle",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Studio",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "MetadataGuid",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMediaItemId",
                table: "MediaVersion",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Genre",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Director",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Artwork",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Actor",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FillerMediaItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FillerMediaItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FillerMediaItem_MediaItem_Id",
                        column: x => x.Id,
                        principalTable: "MediaItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FillerMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FillerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    Product = table.Column<string>(type: "TEXT", nullable: true),
                    MetadataKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: true),
                    SortTitle = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FillerMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FillerMetadata_FillerMediaItem_FillerId",
                        column: x => x.FillerId,
                        principalTable: "FillerMediaItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Writer_FillerMetadataId",
                table: "Writer",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_FillerMetadataId",
                table: "Tag",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitle_FillerMetadataId",
                table: "Subtitle",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Studio_FillerMetadataId",
                table: "Studio",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataGuid_FillerMetadataId",
                table: "MetadataGuid",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaVersion_FillerMediaItemId",
                table: "MediaVersion",
                column: "FillerMediaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_FillerMetadataId",
                table: "Genre",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Director_FillerMetadataId",
                table: "Director",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Artwork_FillerMetadataId",
                table: "Artwork",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Actor_FillerMetadataId",
                table: "Actor",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_FillerMetadata_FillerId",
                table: "FillerMetadata",
                column: "FillerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actor_FillerMetadata_FillerMetadataId",
                table: "Actor",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Artwork_FillerMetadata_FillerMetadataId",
                table: "Artwork",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Director_FillerMetadata_FillerMetadataId",
                table: "Director",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Genre_FillerMetadata_FillerMetadataId",
                table: "Genre",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaVersion_FillerMediaItem_FillerMediaItemId",
                table: "MediaVersion",
                column: "FillerMediaItemId",
                principalTable: "FillerMediaItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataGuid_FillerMetadata_FillerMetadataId",
                table: "MetadataGuid",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Studio_FillerMetadata_FillerMetadataId",
                table: "Studio",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitle_FillerMetadata_FillerMetadataId",
                table: "Subtitle",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_FillerMetadata_FillerMetadataId",
                table: "Tag",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actor_FillerMetadata_FillerMetadataId",
                table: "Actor");

            migrationBuilder.DropForeignKey(
                name: "FK_Artwork_FillerMetadata_FillerMetadataId",
                table: "Artwork");

            migrationBuilder.DropForeignKey(
                name: "FK_Director_FillerMetadata_FillerMetadataId",
                table: "Director");

            migrationBuilder.DropForeignKey(
                name: "FK_Genre_FillerMetadata_FillerMetadataId",
                table: "Genre");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaVersion_FillerMediaItem_FillerMediaItemId",
                table: "MediaVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataGuid_FillerMetadata_FillerMetadataId",
                table: "MetadataGuid");

            migrationBuilder.DropForeignKey(
                name: "FK_Studio_FillerMetadata_FillerMetadataId",
                table: "Studio");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtitle_FillerMetadata_FillerMetadataId",
                table: "Subtitle");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_FillerMetadata_FillerMetadataId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Writer_FillerMetadata_FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropTable(
                name: "FillerMetadata");

            migrationBuilder.DropTable(
                name: "FillerMediaItem");

            migrationBuilder.DropIndex(
                name: "IX_Writer_FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropIndex(
                name: "IX_Tag_FillerMetadataId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Subtitle_FillerMetadataId",
                table: "Subtitle");

            migrationBuilder.DropIndex(
                name: "IX_Studio_FillerMetadataId",
                table: "Studio");

            migrationBuilder.DropIndex(
                name: "IX_MetadataGuid_FillerMetadataId",
                table: "MetadataGuid");

            migrationBuilder.DropIndex(
                name: "IX_MediaVersion_FillerMediaItemId",
                table: "MediaVersion");

            migrationBuilder.DropIndex(
                name: "IX_Genre_FillerMetadataId",
                table: "Genre");

            migrationBuilder.DropIndex(
                name: "IX_Director_FillerMetadataId",
                table: "Director");

            migrationBuilder.DropIndex(
                name: "IX_Artwork_FillerMetadataId",
                table: "Artwork");

            migrationBuilder.DropIndex(
                name: "IX_Actor_FillerMetadataId",
                table: "Actor");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Subtitle");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Studio");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "MetadataGuid");

            migrationBuilder.DropColumn(
                name: "FillerMediaItemId",
                table: "MediaVersion");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Genre");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Director");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Artwork");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Actor");
        }
    }
}
