using Microsoft.EntityFrameworkCore.Migrations;

namespace MontageServer.Migrations
{
    public partial class RenamePathAndIdColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "AdobeProject");

            migrationBuilder.RenameColumn(
                name: "ProjectID",
                table: "AdobeProject",
                newName: "ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "FootagePath",
                table: "AdobeProject",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FootagePath",
                table: "AdobeProject");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "AdobeProject",
                newName: "ProjectID");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "AdobeProject",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
