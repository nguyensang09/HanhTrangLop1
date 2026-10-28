using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonPathProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PathCompletedAt",
                table: "ChildLessonProgresses",
                type: "datetimeoffset",
                nullable: true);

            // Tiến độ hiện tại ban đầu trùng với lịch sử hoàn thành: không bé nào bị đổi vị trí trên lộ trình.
            migrationBuilder.Sql("UPDATE [ChildLessonProgresses] SET [PathCompletedAt] = [FirstCompletedAt] WHERE [FirstCompletedAt] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathCompletedAt",
                table: "ChildLessonProgresses");
        }
    }
}
