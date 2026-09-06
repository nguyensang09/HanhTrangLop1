using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260905000100_ExpandTracingSymbolLength")]
public sealed class ExpandTracingSymbolLength : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Symbol",
            table: "TracingTemplates",
            type: "nvarchar(120)",
            maxLength: 120,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Symbol",
            table: "TracingTemplates",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(120)",
            oldMaxLength: 120);
    }
}
