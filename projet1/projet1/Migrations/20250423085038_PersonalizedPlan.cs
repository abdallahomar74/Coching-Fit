using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projet1.Migrations
{
    /// <inheritdoc />
    public partial class PersonalizedPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalizedPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalizedPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalizedPlans_coachsubscriptions_CoachSubscriptionId",
                        column: x => x.CoachSubscriptionId,
                        principalTable: "coachsubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalizedPlans_CoachSubscriptionId",
                table: "PersonalizedPlans",
                column: "CoachSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalizedPlans");
        }
    }
}
