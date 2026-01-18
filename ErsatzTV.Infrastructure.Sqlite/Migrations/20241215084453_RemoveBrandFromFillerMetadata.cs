using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErsatzTV.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandFromFillerMetadata : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_Writer_FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropIndex(
                name: "IX_Director_FillerMetadataId",
                table: "Director");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Writer");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "Director");

            migrationBuilder.RenameColumn(
                name: "Product",
                table: "FillerMetadata",
                newName: "TVDBId");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "FillerMetadata",
                newName: "TMDBId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "FillerMetadata",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Timeslot",
                table: "FillerMetadata",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "FillerMetadata",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "FillerMediaItem",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    BrandId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FillerMetadata_ProductId",
                table: "FillerMetadata",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FillerMediaItem_FillerMetadataId",
                table: "FillerMediaItem",
                column: "FillerMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_BrandId",
                table: "Product",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_FillerMediaItem_FillerMetadata_FillerMetadataId",
                table: "FillerMediaItem",
                column: "FillerMetadataId",
                principalTable: "FillerMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FillerMetadata_Product_ProductId",
                table: "FillerMetadata",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FillerMediaItem_FillerMetadata_FillerMetadataId",
                table: "FillerMediaItem");

            migrationBuilder.DropForeignKey(
                name: "FK_FillerMetadata_Product_ProductId",
                table: "FillerMetadata");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropIndex(
                name: "IX_FillerMetadata_ProductId",
                table: "FillerMetadata");

            migrationBuilder.DropIndex(
                name: "IX_FillerMediaItem_FillerMetadataId",
                table: "FillerMediaItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "FillerMetadata");

            migrationBuilder.DropColumn(
                name: "Timeslot",
                table: "FillerMetadata");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "FillerMetadata");

            migrationBuilder.DropColumn(
                name: "FillerMetadataId",
                table: "FillerMediaItem");

            migrationBuilder.RenameColumn(
                name: "TVDBId",
                table: "FillerMetadata",
                newName: "Product");

            migrationBuilder.RenameColumn(
                name: "TMDBId",
                table: "FillerMetadata",
                newName: "Brand");

            migrationBuilder.AddColumn<int>(
                name: "FillerMetadataId",
                table: "Writer",
                type: "INTEGER",
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
