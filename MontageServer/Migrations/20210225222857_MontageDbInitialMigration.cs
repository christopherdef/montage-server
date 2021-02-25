using Microsoft.EntityFrameworkCore.Migrations;

namespace MontageServer.Migrations
{
    public partial class MontageDbInitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectCache",
                columns: table => new
                {
                    ProjectID = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    AudioResponseString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCache", x => x.ProjectID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectCache");
        }
    }
}
