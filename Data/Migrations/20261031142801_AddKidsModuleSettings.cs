using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKidsModuleSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KidsModuleSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsModuleSettings", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidsModuleSettings");
        }
    }
}
