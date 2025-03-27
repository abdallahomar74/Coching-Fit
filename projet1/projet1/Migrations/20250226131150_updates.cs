using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using static System.Net.Mime.MediaTypeNames;

#nullable disable

namespace projet1.Migrations
{
    /// <inheritdoc />
    public partial class updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                  table: "AspNetRoles",
                  columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                  values: new object[] { Guid.NewGuid().ToString(), "Coach", "Coach".ToUpper(), Guid.NewGuid().ToString() }
                  );

            migrationBuilder.AddColumn<byte[]>(
                name:"Image",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [AspNetRoles] WHERE Name = 'Coach'");
            migrationBuilder.DropColumn(
                name: "Image",
                table: "AspNetUsers");
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");
        }

  
    }
}
