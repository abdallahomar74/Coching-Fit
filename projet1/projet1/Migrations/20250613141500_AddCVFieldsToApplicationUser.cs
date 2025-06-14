using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projet1.Migrations
{
    /// <inheritdoc />
    public partial class AddCVFieldsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVDownloadUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVFileId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVFileName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CVUploadDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVViewUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVDownloadUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CVFileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CVFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CVUploadDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CVViewUrl",
                table: "AspNetUsers");
        }
    }
}
