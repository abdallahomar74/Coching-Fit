using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projet1.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleDriveToPersonalizedPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "PersonalizedPlans",
                newName: "GoogleDriveUrl");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "PersonalizedPlans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "GoogleDriveFileId",
                table: "PersonalizedPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "PersonalizedPlans");

            migrationBuilder.DropColumn(
                name: "GoogleDriveFileId",
                table: "PersonalizedPlans");

            migrationBuilder.RenameColumn(
                name: "GoogleDriveUrl",
                table: "PersonalizedPlans",
                newName: "FilePath");
        }
    }
}
