using Microsoft.EntityFrameworkCore.Migrations;

namespace MontageServer.Migrations
{
    public partial class AddSpeakerProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClipAssignment_AdobeProject_ProjectId_Id",
                table: "ClipAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClipAssignment",
                table: "ClipAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ClipAssignment_ProjectId_Id",
                table: "ClipAssignment");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ClipAssignment");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ClipAssignment",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Error",
                table: "AnalysisResult",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClipAssignment",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "ClipId", "UserId" });

            migrationBuilder.CreateTable(
                name: "SpeakerProfiles",
                columns: table => new
                {
                    SpeakerId = table.Column<string>(nullable: false),
                    ProjectId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ModelPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakerProfiles", x => new { x.SpeakerId, x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SpeakerProfiles_AdobeProject_ProjectId_UserId",
                        columns: x => new { x.ProjectId, x.UserId },
                        principalTable: "AdobeProject",
                        principalColumns: new[] { "ProjectId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClipAssignment_ProjectId_UserId",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakerProfiles_ProjectId_UserId",
                table: "SpeakerProfiles",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClipAssignment_AdobeProject_ProjectId_UserId",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "UserId" },
                principalTable: "AdobeProject",
                principalColumns: new[] { "ProjectId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClipAssignment_AdobeProject_ProjectId_UserId",
                table: "ClipAssignment");

            migrationBuilder.DropTable(
                name: "SpeakerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClipAssignment",
                table: "ClipAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ClipAssignment_ProjectId_UserId",
                table: "ClipAssignment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ClipAssignment");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "AnalysisResult");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ClipAssignment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClipAssignment",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "ClipId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClipAssignment_ProjectId_Id",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClipAssignment_AdobeProject_ProjectId_Id",
                table: "ClipAssignment",
                columns: new[] { "ProjectId", "Id" },
                principalTable: "AdobeProject",
                principalColumns: new[] { "ProjectId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
