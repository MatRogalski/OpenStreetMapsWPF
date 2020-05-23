using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace DbConnector.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizationPoints",
                columns: table => new
                {
                    PointId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Coordinate = table.Column<Point>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    District = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Street = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: true),
                    StaticScore = table.Column<double>(nullable: true),
                    InnerDistance = table.Column<double>(nullable: true),
                    InnerTime = table.Column<double>(nullable: true),
                    ParentPointId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationPoints", x => x.PointId);
                    table.ForeignKey(
                        name: "FK_LocalizationPoints_LocalizationPoints_ParentPointId",
                        column: x => x.ParentPointId,
                        principalTable: "LocalizationPoints",
                        principalColumn: "PointId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationPoints_ParentPointId",
                table: "LocalizationPoints",
                column: "ParentPointId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalizationPoints");
        }
    }
}
