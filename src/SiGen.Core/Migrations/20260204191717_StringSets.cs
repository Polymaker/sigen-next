using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiGen.Migrations
{
    /// <inheritdoc />
    public partial class StringSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StringSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    InstrumentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NumberOfStrings = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StringSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MaterialType = table.Column<string>(type: "TEXT", nullable: false),
                    Gauge = table.Column<double>(type: "REAL", nullable: false),
                    CoreDiameter = table.Column<double>(type: "REAL", nullable: true),
                    UnitWeight = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringSpecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StringSetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StringSetId = table.Column<int>(type: "INTEGER", nullable: false),
                    StringSpecId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StringSetItems_StringSets_StringSetId",
                        column: x => x.StringSetId,
                        principalTable: "StringSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StringSetItems_StringSpecs_StringSpecId",
                        column: x => x.StringSpecId,
                        principalTable: "StringSpecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StringSetItems_StringSetId",
                table: "StringSetItems",
                column: "StringSetId");

            migrationBuilder.CreateIndex(
                name: "IX_StringSetItems_StringSpecId",
                table: "StringSetItems",
                column: "StringSpecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StringSetItems");

            migrationBuilder.DropTable(
                name: "StringSets");

            migrationBuilder.DropTable(
                name: "StringSpecs");
        }
    }
}
