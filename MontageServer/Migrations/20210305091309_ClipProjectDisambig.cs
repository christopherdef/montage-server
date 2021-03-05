using Microsoft.EntityFrameworkCore.Migrations;

namespace MontageServer.Migrations
{
    public partial class ClipProjectDisambig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdobeProject",
                table: "AdobeProject");

            migrationBuilder.DropColumn(
                name: "AnalysisResultString",
                table: "AdobeProject");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "AdobeProject");

            migrationBuilder.RenameColumn(
                name: "ProjectID",
                table: "AdobeProject",
                newName: "ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AdobeProject",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdobeProject",
                table: "AdobeProject",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateTable(
                name: "AdobeClip",
                columns: table => new
                {
                    ClipId = table.Column<string>(nullable: false),
                    FootagePath = table.Column<string>(nullable: true),
                    AnalysisResultString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdobeClip", x => x.ClipId);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResult",
                columns: table => new
                {
                    ClipId = table.Column<string>(nullable: false),
                    Transcript = table.Column<string>(nullable: true),
                    FootagePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResult", x => x.ClipId);
                });

            migrationBuilder.CreateTable(
                name: "ClipAssignment",
                columns: table => new
                {
                    ClipId = table.Column<string>(nullable: false),
                    ProjectId = table.Column<string>(nullable: false),
                    Id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipAssignment", x => new { x.ProjectId, x.ClipId });
                    table.ForeignKey(
                        name: "FK_ClipAssignment_AdobeClip_ClipId",
                        column: x => x.ClipId,
                        principalTable: "AdobeClip",
                        principalColumn: "ClipId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClipAssignment_AdobeProject_ProjectId_Id",
                        columns: x => new { x.ProjectId, x.Id },
                        principalTable: "AdobeProject",
                        principalColumns: new[] { "ProjectId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClipAssignment_ClipId",
                table: "ClipAssignment",
                column: "ClipId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipAssignment_ProjectId_Id",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "Id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResult");

            migrationBuilder.DropTable(
                name: "ClipAssignment");

            migrationBuilder.DropTable(
                name: "AdobeClip");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdobeProject",
                table: "AdobeProject");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdobeProject");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "AdobeProject",
                newName: "ProjectID");

            migrationBuilder.AddColumn<string>(
                name: "AnalysisResultString",
                table: "AdobeProject",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "AdobeProject",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdobeProject",
                table: "AdobeProject",
                column: "ProjectID");
        }
    }
}
