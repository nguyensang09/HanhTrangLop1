using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260906000100_AddMediaAssetCategory")]
public sealed class AddMediaAssetCategory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "MediaAssets",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Category",
            table: "MediaAssets");
    }
}
