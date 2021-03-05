using Microsoft.EntityFrameworkCore.Migrations;

namespace MontageServer.Migrations
{
    public partial class CacheTableRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectCache");

            migrationBuilder.CreateTable(
                name: "AdobeProject",
                columns: table => new
                {
                    ProjectID = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    AnalysisResultString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdobeProject", x => x.ProjectID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdobeProject");

            migrationBuilder.CreateTable(
                name: "ProjectCache",
                columns: table => new
                {
                    ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnalysisResultString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCache", x => x.ProjectId);
                });
        }
    }
}
