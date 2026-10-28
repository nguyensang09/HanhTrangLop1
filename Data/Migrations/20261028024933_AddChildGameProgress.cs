using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChildGameProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChildGameProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    BestStars = table.Column<int>(type: "int", nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    FirstCompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastPlayedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProgressEpoch = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildGameProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildGameProgresses_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildGameProgresses_ChildProfileId_ProgressEpoch_GameKey_Level",
                table: "ChildGameProgresses",
                columns: new[] { "ChildProfileId", "ProgressEpoch", "GameKey", "Level" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildGameProgresses");
        }
    }
}
